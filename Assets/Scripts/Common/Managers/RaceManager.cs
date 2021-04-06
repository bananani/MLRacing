using System;
using System.Collections.Generic;
using System.Text;
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

        [SerializeField]
        private int _laps;

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

            SetupRace(_currentTrack, _currentTrack.SelectedTrackVariant, _laps, _entrants);
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

        private void OnCheckpointReport(Checkpoint checkpoint, Transponder transponder, InfractionSeverityIdentifier infractionSeverity)
        {
            if(!_lapDataLookup.TryGetValue(transponder.TransponderId, out LapData lapData))
            {
                Debug.LogError($"Lapdata not found for transponder {transponder.TransponderId}");
                return;
            }

            InfractionData infractionData =
                new InfractionData(
                    checkpoint.CheckpointIndex + 1,
                    transponder.CurrentTime - _raceStartTime,
                    transponder.GetCarSpeed(),
                    transponder.GetCarAcceleration(),
                    infractionSeverity
                );

            lapData.CheckpointPassed(checkpoint.CheckpointIndex, infractionData);

            if(!checkpoint.IsFinishLine || !lapData.TryCompleteLap(_currentTrack.CheckpointCount, Time.time, out int completedLaps) || !_entrantLookup.TryGetValue(transponder.TransponderId, out RaceEntrant entrant))
            {
                return;
            }

            // Car completed lap
            if(lapData.GetPreviousLapTime().HasValue)
            {
                (float time, bool valid) = lapData.GetPreviousLapTime().Value;
                Debug.Log($"{entrant.Driver.Name} completed lap #{completedLaps - 1} in time {time} (Lap was valid: {valid})");
            }

            if(_lapCount <= 0 || completedLaps != _lapCount)
            {
                return;
            }

            // Car has finished race
            _finishedDrivers.Add((entrant, lapData));
            _activeTransponders.Remove(transponder);
            lapData.StopRaceTimer(Time.time);

            StringBuilder lapsString = new StringBuilder();
            lapsString.AppendLine();
            lapsString.AppendLine();
            foreach(KeyValuePair<int, (float time, bool valid)> lap in lapData.LapTimes)
            {
                if(lap.Value.valid)
                {
                    lapsString.AppendLine($"Lap {lap.Key + 1}: {lap.Value.time} s");
                }
                else
                {
                    lapsString.AppendLine($"!InvalidLap!: {lap.Value.time} s");
                }
            }

            StringBuilder infractionsString = new StringBuilder();

            if(lapData.InfractionsByLap != null && lapData.InfractionsByLap.Count > 0)
            {
                infractionsString.AppendLine($"\n\n{lapData.TotalInfractions} Infractions");
                foreach(int lap in lapData.InfractionsByLap.Keys)
                {
                    infractionsString.AppendLine($"\tLap {lap + 1}");
                    foreach(InfractionData infraction in lapData.InfractionsByLap[lap])
                    {
                        infractionsString.AppendLine($"\t\t{infraction.TimeOfInfraction:0.00} s || Checkpoint #{infraction.CheckpointIndex} : {infraction.Severity} infraction at {infraction.Speed:0.00} kmh");
                    }
                }
            }

            Debug.Log($"{entrant.Driver.Name} completed race in time {lapData.TotalRaceTime}, position {_finishedDrivers.Count} / {_raceGrid.Count}{lapsString}{infractionsString}");

            if(_activeTransponders.Count > 0)
            {
                return;
            }

            // Race over
            StringBuilder standingsString = new StringBuilder();
            for(int i = 0; i < _finishedDrivers.Count; i++)
            {
                standingsString.AppendLine($"{i + 1} : {_finishedDrivers[i].entrant.Driver.Name} (Fastest lap: {_finishedDrivers[i].lapData.FastestLap})");
            }

            Debug.Log($"Race is over!\n{standingsString}");
            RaceEnded?.Invoke();
        }
    }
}