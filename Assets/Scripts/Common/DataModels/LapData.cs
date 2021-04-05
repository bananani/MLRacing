using System.Linq;
using System;
using System.Collections.Generic;

namespace Common.DataModels
{
    public class LapData
    {
        public int LatestCheckpoint { get; private set; }
        public int CurrentLap { get; private set; }

        public readonly HashSet<int> PassedCheckpoints = new HashSet<int>();

        private float raceStartTime;
        private float raceEndTime;
        private float currentLapStartTime;

        public readonly Dictionary<int, (float time, bool valid)> LapTimes = new Dictionary<int, (float, bool)>();

        public float FastestLap => LapTimes.Values.Min(l => l.time);

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

        public void CheckpointPassed(int index)
        {
            LatestCheckpoint = index + 1;
            PassedCheckpoints.Add(LatestCheckpoint);
        }

        public bool TryCompleteLap(int checkpointAmount, float lapEndTime, bool countInvalidLap, out int lapCount)
        {
            bool lapIsValid = IsLapValid(checkpointAmount);

            if(lapIsValid || countInvalidLap)
            {
                LapTimes.Add(CurrentLap, (lapEndTime - currentLapStartTime, lapIsValid));
                CurrentLap++;
                StartLapTimer(lapEndTime);
                PassedCheckpoints.Clear();
            }

            lapCount = CurrentLap;
            return lapIsValid;
        }

        public bool IsLapValid(int checkpointAmount)
        {
            int sumOfCheckpointIds = (checkpointAmount * (checkpointAmount + 1)) / 2;

            int sumOfPassedCheckpointIndexes = 0;
            foreach(int index in PassedCheckpoints)
            {
                sumOfPassedCheckpointIndexes += index;
            }

            return sumOfCheckpointIds == sumOfPassedCheckpointIndexes;
        }
    }
}