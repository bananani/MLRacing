using System;
using System.Collections.Generic;
using Common.Components;
using Common.Components.TrackParts;
using Common.DataModels;
using Common.Identifiers;
using UnityEngine;

namespace Common.Managers
{
    public class RaceManager : MonoBehaviour
    {
        public delegate void RaceEvent();
        public event RaceEvent RaceStarted;
        public event RaceEvent RaceEnded;

        private int _lapCount;
        private List<RaceEntrant> _raceGrid;
        private readonly Dictionary<Guid, Transponder> _transponderLookup = new Dictionary<Guid, Transponder>();
        private readonly Dictionary<Guid, RaceEntrant> _entrantLookup = new Dictionary<Guid, RaceEntrant>();
        private readonly Dictionary<Guid, LapData> _lapDataLookup = new Dictionary<Guid, LapData>();

        private readonly HashSet<Transponder> _activeTransponders = new HashSet<Transponder>();
        private readonly List<(RaceEntrant entrant, LapData lapData)> _finishedDrivers = new List<(RaceEntrant, LapData)>();

        private float _raceStartTime;

        [SerializeField]
        private TrackManager _currentTrack;
        [SerializeField]
        private List<RaceEntrant> _entrants;

        public void Awake()
        {
            if(_currentTrack == null)
            {
                return;
            }

            SetupRace(_currentTrack, _currentTrack.SelectedTrackVariant, 2, _entrants);
            StartRace(Time.time);
        }

        public void SetupRace(TrackManager track, int trackVariantId, int laps, List<RaceEntrant> entrants)
        {
            _lapCount = laps;
            _raceGrid = entrants;
            _currentTrack = track;

            _currentTrack.InitializeTrackVariant(trackVariantId);
            foreach(Checkpoint checkpoint in _currentTrack.CurrentTrackVariant.Checkpoints)
            {
                checkpoint.CheckpointReport += OnCheckpointReport;
            }

            for(int i = 0; i < _raceGrid.Count; i++)
            {
                RaceEntrant entrant = _raceGrid[i];
                entrant.Car.Init();
                entrant.Car.SetupCarForDriver(entrant.Driver);
                entrant.Car.PlaceOnGrid(_currentTrack.CurrentTrackVariant.GetGridPositionTransform(i + 1));
                Transponder transponder = entrant.Car.SetupTransponder();

                Guid transponderId = transponder.TransponderId;
                _transponderLookup[transponderId] = transponder;
                _entrantLookup[transponderId] = entrant;
                _lapDataLookup[transponderId] = new LapData();

                _activeTransponders.Add(transponder);
            }
        }

        public void StartRace(float raceStartTime)
        {
            _raceStartTime = raceStartTime;
            foreach(LapData lapData in _lapDataLookup.Values)
            {
                lapData.StartRaceTimer(raceStartTime);
                lapData.StartLapTimer(raceStartTime);
            }

            Debug.Log($"Race started! Entrant count: {_raceGrid.Count}");
            RaceStarted?.Invoke();
        }

        private void OnCheckpointReport(Checkpoint checkpoint, Transponder transponder, OffTrackSeverityIdentifier infractionSeverity)
        {
            if(!_lapDataLookup.TryGetValue(transponder.TransponderId, out LapData lapData))
            {
                Debug.LogError($"Lapdata not found for transponder {transponder.TransponderId}");
                return;
            }

            lapData.CheckpointPassed(checkpoint.CheckpointIndex);

            if(!checkpoint.IsFinishLine || !lapData.TryCompleteLap(_currentTrack.CheckpointCount, Time.time, false, out int completedLaps) || !_entrantLookup.TryGetValue(transponder.TransponderId, out RaceEntrant entrant))
            {
                return;
            }

            // Car completed lap
            if(lapData.GetPreviousLapTime().HasValue)
            {
                (float time, bool valid) lap = lapData.GetPreviousLapTime().Value;
                Debug.Log($"{entrant.Driver.Name} completed lap #{completedLaps - 1} in time {lap.time} (Lap was valid: {lap.valid})");
            }

            if(_lapCount <= 0 || completedLaps != _lapCount)
            {
                return;
            }

            // Car has finished race
            _finishedDrivers.Add((entrant, lapData));
            _activeTransponders.Remove(transponder);
            lapData.StopRaceTimer(Time.time);

            Debug.Log($"{entrant.Driver.Name} completed race in time {lapData.TotalRaceTime}, position {_finishedDrivers.Count} / {_raceGrid.Count}");

            if(_activeTransponders.Count > 0)
            {
                return;
            }

            // Race over
            RaceEnded?.Invoke();
        }
    }
}