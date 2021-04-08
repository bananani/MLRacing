using System.Linq;
using System.Collections.Generic;
using Common.Identifiers;

namespace Common.DataModels
{
    public class LapData
    {
        public int LatestCheckpoint { get; private set; }
        public int CurrentLap { get; private set; }

        public readonly HashSet<int> PassedCheckpoints = new HashSet<int>();
        public readonly Dictionary<int, Dictionary<int, float>> SectorTimesBylap = new Dictionary<int, Dictionary<int, float>>();
        public readonly Dictionary<int, List<InfractionData>> InfractionsByLap = new Dictionary<int, List<InfractionData>>();
        public int TotalInfractions => InfractionsByLap.Sum(kvp => kvp.Value.Count);

        private float _raceStartTime;
        private float _raceEndTime;
        private float _currentLapStartTime;
        private float _previousSectorTime;
        private bool _currentLapInvalid;

        public readonly Dictionary<int, (float time, bool valid)> LapTimes = new Dictionary<int, (float, bool)>();


        public float FastestLap => LapTimes.Values.Min(l => l.valid ? l.time : float.MaxValue);
        public Dictionary<int, float> GetFastestSectors()
        {
            HashSet<int> validLaps = new HashSet<int>();
            foreach(KeyValuePair<int, (float time, bool valid)> kvp in LapTimes)
            {
                if(kvp.Value.valid)
                {
                    validLaps.Add(kvp.Key);
                }
            }

            Dictionary<int, float> fastestSectors = new Dictionary<int, float>();
            foreach(KeyValuePair<int, Dictionary<int, float>> kvp in SectorTimesBylap)
            {
                if(!validLaps.Contains(kvp.Key))
                {
                    continue;
                }

                foreach(KeyValuePair<int, float> sector in kvp.Value)
                {
                    if(!fastestSectors.ContainsKey(sector.Key))
                    {
                        fastestSectors.Add(sector.Key, sector.Value);
                        continue;
                    }

                    if(fastestSectors[sector.Key] < sector.Value)
                    {
                        continue;
                    }

                    fastestSectors[sector.Key] = sector.Value;
                }
            }

            return fastestSectors;
        }

        public float TheoreticalFastestLap => GetFastestSectors().Sum(kvp => kvp.Value);

        public void StartRaceTimer(float startTime) { _raceStartTime = startTime; _previousSectorTime = _raceStartTime; }
        public void StartLapTimer(float startTime) => _currentLapStartTime = startTime;
        public void StopRaceTimer(float endTime) => _raceEndTime = endTime;
        public void InvalidateLap() => _currentLapInvalid = true;
        public float TotalRaceTime => _raceEndTime - _raceStartTime;

        public (float time, bool valid)? GetPreviousLapTime()
        {
            if(LapTimes.TryGetValue(CurrentLap - 1, out (float time, bool valid) lapData))
            {
                return lapData;
            }

            return null;
        }

        public void CheckpointPassed(int index, InfractionData infraction)
        {
            if(infraction.Severity == InfractionSeverityIdentifier.OnTrack)
            {
                LatestCheckpoint = index + 1;
                PassedCheckpoints.Add(LatestCheckpoint);
                return;
            }

            if(!InfractionsByLap.ContainsKey(CurrentLap))
            {
                InfractionsByLap.Add(CurrentLap, new List<InfractionData>());
            }

            InfractionsByLap[CurrentLap].Add(infraction);
        }

        public void AddSectorTime(int sector, float time)
        {
            if(!SectorTimesBylap.ContainsKey(CurrentLap))
            {
                SectorTimesBylap.Add(CurrentLap, new Dictionary<int, float>());
            }

            if(sector != 1 && SectorTimesBylap[CurrentLap].Count == 0)
            {
                // Crossing a sector limit before it's supposed to be crossed, such as finish line
                return;
            }

            if(SectorTimesBylap[CurrentLap].ContainsKey(sector))
            {
                return;
            }

            float sectorTime = time - _previousSectorTime;
            SectorTimesBylap[CurrentLap].Add(sector, sectorTime);
            _previousSectorTime = time;

            UnityEngine.Debug.Log($"Sector #{sector}: {sectorTime}");
        }

        public bool TryCompleteLap(int checkpointAmount, float lapEndTime, out int lapCount)
        {
            bool lapIsValid = IsLapValid(checkpointAmount, out bool lapComplete);

            if(!lapComplete)
            {
                lapCount = CurrentLap;
                return false;
            }

            LapTimes.Add(CurrentLap, (lapEndTime - _currentLapStartTime, lapIsValid));
            CurrentLap++;
            StartLapTimer(lapEndTime);
            PassedCheckpoints.Clear();
            _currentLapInvalid = false;

            lapCount = CurrentLap;
            return lapComplete;
        }

        public bool IsLapValid(int checkpointAmount, out bool lapComplete)
        {
            lapComplete = false;
            int sumOfCheckpointIds = (checkpointAmount * (checkpointAmount + 1)) / 2;

            // Passed valid checkpoints
            HashSet<int> passedCheckpoints = new HashSet<int>();
            foreach(int index in PassedCheckpoints)
            {
                passedCheckpoints.Add(index);
            }

            // If all checkpoints have been passed, we've completed the lap (mostly) on road. Hoorray
            if(sumOfCheckpointIds == passedCheckpoints.Sum())
            {
                lapComplete = true;

                // Lap has been manually invalidated because car got completely off trtack at some point
                return !_currentLapInvalid;
            }

            // If lap is not complete on road and we don't have any infractions on the lap, we've taken a shortcut
            if(!InfractionsByLap.ContainsKey(CurrentLap))
            {
                return false;
            }

            // Infractions
            foreach(InfractionData infraction in InfractionsByLap[CurrentLap])
            {
                passedCheckpoints.Add(infraction.CheckpointIndex);
            }

            if(sumOfCheckpointIds == passedCheckpoints.Sum())
            {
                lapComplete = true;
                return false;
            }

            return false;
        }
    }
}