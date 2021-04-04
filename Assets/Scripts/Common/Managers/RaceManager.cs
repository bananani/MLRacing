using System;
using System.Collections.Generic;
using Common.Components;
using Common.Components.TrackParts;
using Common.DataModels;
using Common.Identifiers;

namespace Common.Managers
{
    public class RaceManager
    {
        public delegate void RaceEvent();
        public event RaceEvent RaceStarted;
        public event RaceEvent RaceEnded;

        private int _lapCount;
        private List<RaceEntrant> _raceGrid;
        private readonly Dictionary<Transponder, RaceEntrant> _entrantLookup = new Dictionary<Transponder, RaceEntrant>();
        private readonly Dictionary<Transponder, LapData> _lapDataLookup = new Dictionary<Transponder, LapData>();

        private readonly HashSet<Transponder> _activeTransponders = new HashSet<Transponder>();
        private readonly List<(RaceEntrant entrant, LapData lapData)> _finishedDrivers = new List<(RaceEntrant, LapData)>();

        private TrackManager _currentTrack;

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
                entrant.Car.SetupCarForDriver(entrant.Driver);
                entrant.Car.PlaceOnGrid(_currentTrack.CurrentTrackVariant.GetGridPositionTransform(i + 1));

                Transponder transponder = entrant.Car.SetupTransponder();
                _entrantLookup[transponder] = entrant;
                _lapDataLookup[transponder] = new LapData();
                _activeTransponders.Add(transponder);
            }
        }

        public void StartRace(float raceStartTime)
        {
            foreach(LapData lapData in _lapDataLookup.Values)
            {
                lapData.StartLapTimer(raceStartTime);
            }

            RaceStarted?.Invoke();
        }

        private void OnCheckpointReport(Checkpoint checkpoint, Transponder transponder, OffTrackSeverityIdentifier infractionSeverity)
        {
            if(!_lapDataLookup.TryGetValue(transponder, out LapData lapData))
            {
                return;
            }

            lapData.CheckpointPassed(checkpoint.CheckpointIndex);

            if(!checkpoint.IsFinishLine || !lapData.TryCompleteLap(_currentTrack.CheckpointCount, 0f, false, out int completedLaps) || _lapCount <= 0 || completedLaps != _lapCount || !_entrantLookup.TryGetValue(transponder, out RaceEntrant entrant))
            {
                return;
            }

            // Car has finished race
            _finishedDrivers.Add((entrant, lapData));
            _activeTransponders.Remove(transponder);

            if(_activeTransponders.Count > 0)
            {
                return;
            }

            // Race over
            RaceEnded?.Invoke();
        }
    }
}