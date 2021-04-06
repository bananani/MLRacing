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
        public readonly Dictionary<int, List<InfractionData>> InfractionsByLap = new Dictionary<int, List<InfractionData>>();
        public int TotalInfractions => InfractionsByLap.Sum(kvp => kvp.Value.Count);

        private float raceStartTime;
        private float raceEndTime;
        private float currentLapStartTime;

        public readonly Dictionary<int, (float time, bool valid)> LapTimes = new Dictionary<int, (float, bool)>();

        public int ValidLapCount => LapTimes.Count(kvp => kvp.Value.valid);
        public float FastestLap => LapTimes.Values.Min(l => l.valid ? l.time : float.MaxValue);

        public void StartRaceTimer(float startTime) => raceStartTime = startTime;
        public void StartLapTimer(float startTime) => currentLapStartTime = startTime;
        public void StopRaceTimer(float endTime) => raceEndTime = endTime;
        public float TotalRaceTime => raceEndTime - raceStartTime;

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

        public bool TryCompleteLap(int checkpointAmount, float lapEndTime, out int lapCount)
        {
            bool lapIsValid = IsLapValid(checkpointAmount, out bool lapComplete);

            if(!lapComplete)
            {
                lapCount = ValidLapCount;
                return false;
            }

            LapTimes.Add(CurrentLap, (lapEndTime - currentLapStartTime, lapIsValid));
            CurrentLap++;
            StartLapTimer(lapEndTime);
            PassedCheckpoints.Clear();

            lapCount = ValidLapCount;
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

            if(sumOfCheckpointIds == passedCheckpoints.Sum())
            {
                lapComplete = true;
                return true;
            }

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