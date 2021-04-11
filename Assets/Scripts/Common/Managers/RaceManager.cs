using System;
using System.Collections.Generic;
using System.Text;
using Common.Components;
using Common.Components.TrackParts;
using Common.Constants;
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

        private readonly Dictionary<Guid, List<InfractionData>> _driversUnderInvestigation = new Dictionary<Guid, List<InfractionData>>();

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
                transponder.LapInvalidated += OnLapInvalidated;

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

        private void EvaluatePenalties(Guid driverId, InfractionData violationData)
        {
            if(!_lapDataLookup.TryGetValue(driverId, out LapData lapData))
            {
                return;
            }

            // Evaluate existing or add new violations
            if(!_driversUnderInvestigation.ContainsKey(driverId))
            {
                // New violation
                if(violationData.Severity == InfractionSeverityIdentifier.InCornerApex)
                {
                    if(lapData.PassedCheckpoints.Contains(violationData.CheckpointIndex))
                    {
                        // The driver has been on the track at this point, should be fine...
                        return;
                    }

                    // We need more info to determine if the current trajectory of the car is punishable
                    _driversUnderInvestigation.Add(driverId, new List<InfractionData>() { violationData });
                }
            }
            else
            {
                // New information received for the driver track limit violation
                switch(violationData.Severity)
                {
                    case InfractionSeverityIdentifier.OnTrack:
                        // Car has returned to the track (even if it is just for a while), so try to resolve all pending violations
                        List<int> resolvedViolations = new List<int>();

                        // Resolve violations, where player has been partially on track 
                        for(int i = 0; i < _driversUnderInvestigation[driverId].Count; i++)
                        {
                            InfractionData incident = _driversUnderInvestigation[driverId][i];
                            if(lapData.PassedCheckpoints.Contains(incident.CheckpointIndex))
                            {
                                // False alarm, the guy's been drifting close enough to the track
                                resolvedViolations.Add(i);
                                continue;
                            }
                        }

                        for(int i = resolvedViolations.Count - 1; i >= 0; i--)
                        {
                            _driversUnderInvestigation[driverId].RemoveAt(resolvedViolations[i]);
                        }

                        // Check if player has unresolved violations
                        if(_driversUnderInvestigation[driverId].Count > 0)
                        {
                            InfractionSeverityIdentifier highestSeverity = InfractionSeverityIdentifier.OnTrack;

                            float timeOfFirstViolation = float.MaxValue;
                            float distanceCoveredDuringViolation = 0f;
                            float minSpeed = float.MaxValue;
                            float maxSpeed = float.MinValue;
                            InfractionData? previousViolation = null;

                            foreach(InfractionData violation in _driversUnderInvestigation[driverId])
                            {
                                if(violation.Severity > highestSeverity)
                                {
                                    highestSeverity = violation.Severity;
                                }

                                if(violation.TimeOfInfraction < timeOfFirstViolation)
                                {
                                    timeOfFirstViolation = violation.TimeOfInfraction;
                                }

                                if(previousViolation.HasValue)
                                {
                                    CalculateViolationData(previousViolation.Value, violation, ref distanceCoveredDuringViolation, ref minSpeed, ref maxSpeed);
                                }

                                previousViolation = violation;
                            }

                            CalculateViolationData(previousViolation.Value, violationData, ref distanceCoveredDuringViolation, ref minSpeed, ref maxSpeed);

                            float totalViolationDuration = violationData.TimeOfInfraction - timeOfFirstViolation;

                            // Average velocity in meters per second
                            float averageSpeedDuringViolation = distanceCoveredDuringViolation / totalViolationDuration;

                            if(highestSeverity > InfractionSeverityIdentifier.OffTrack)
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine("Driver will receive a penalty for an off-track violation during a race");
                                sb.AppendLine($"Amount of individual track limit infractions: {_driversUnderInvestigation[driverId].Count}");
                                sb.AppendLine($"Highest severity: {highestSeverity}");
                                sb.AppendLine($"Duration: {totalViolationDuration:0.00} s");
                                sb.AppendLine($"Distance covered: {distanceCoveredDuringViolation:0.00} m");
                                sb.AppendLine($"Average speed: {averageSpeedDuringViolation:0.00} m/s ({averageSpeedDuringViolation * CVelocity.MS_TO_KMH_CONVERSION:0.00} kmh)");
                                sb.AppendLine($"Min speed: {minSpeed:0.00} m/s ({minSpeed * CVelocity.MS_TO_KMH_CONVERSION:0.00} kmh)");
                                sb.AppendLine($"Max speed: {maxSpeed:0.00} m/s ({maxSpeed * CVelocity.MS_TO_KMH_CONVERSION:0.00} kmh)");
                                Debug.LogError(sb);
                            }

                            _driversUnderInvestigation[driverId].Clear();
                        }

                        return;

                    default:
                        if(lapData.PassedCheckpoints.Contains(violationData.CheckpointIndex))
                        {
                            // The driver has been on the track at this point, should be fine...
                            return;
                        }

                        // Add new information about the current violation
                        _driversUnderInvestigation[driverId].Add(violationData);
                        break;
                }
            }
        }

        private void CalculateViolationData(InfractionData previousViolation, InfractionData violation, ref float distanceCoveredDuringViolation, ref float minSpeed, ref float maxSpeed)
        {
            float coveredDistance = Vector2.Distance(previousViolation.CarPosition, violation.CarPosition);
            float duration = violation.TimeOfInfraction - previousViolation.TimeOfInfraction;

            distanceCoveredDuringViolation += coveredDistance;
            float speed = coveredDistance / duration;

            if(violation.Speed < minSpeed)
            {
                minSpeed = violation.Speed;
            }

            if(violation.Speed > maxSpeed)
            {
                minSpeed = violation.Speed;
            }

            if(speed < minSpeed)
            {
                minSpeed = speed;
            }

            if(speed > maxSpeed)
            {
                maxSpeed = speed;
            }
        }

        private void OnLapInvalidated(Transponder transponder)
        {
            if(!_lapDataLookup.TryGetValue(transponder.TransponderId, out LapData lapData))
            {
                return;
            }

            lapData.InvalidateLap();
        }

        private void OnCheckpointReport(Checkpoint checkpoint, Transponder transponder, InfractionSeverityIdentifier infractionSeverity)
        {
            if(!_lapDataLookup.TryGetValue(transponder.TransponderId, out LapData lapData))
            {
                Debug.LogError($"Lapdata not found for transponder {transponder.TransponderId}");
                return;
            }

            float currentTime = transponder.CurrentTime;

            InfractionData infractionData =
                new InfractionData(
                    checkpoint.CheckpointIndex + 1,
                    currentTime - _raceStartTime,
                    transponder.transform.position,
                    transponder.GetCarSpeed(),
                    transponder.GetCarAcceleration(),
                    infractionSeverity
                );

            lapData.CheckpointPassed(checkpoint.CheckpointIndex, infractionData);
            if((checkpoint.IsSectorCheckpoint || checkpoint.IsFinishLine) && _currentTrack.TryGetSectorIndex(checkpoint, out int sector))
            {
                lapData.AddSectorTime(sector, currentTime);
            }

            if(infractionSeverity != InfractionSeverityIdentifier.OnTrack || _driversUnderInvestigation.ContainsKey(transponder.TransponderId))
            {
                EvaluatePenalties(transponder.TransponderId, infractionData);
            }

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
                standingsString.AppendLine($"{i + 1} : {_finishedDrivers[i].entrant.Driver.Name} (Fastest lap: {_finishedDrivers[i].lapData.FastestLap}, Fastest theoretical lap: {_finishedDrivers[i].lapData.TheoreticalFastestLap})");
            }

            Debug.Log($"Race is over!\n{standingsString}");
            RaceEnded?.Invoke();
        }
    }
}