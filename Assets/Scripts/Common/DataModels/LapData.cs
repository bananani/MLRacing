using System;
using System.Collections.Generic;

namespace Common.DataModels
{
    public class LapData
    {
        public int LatestCheckpoint { get; private set; }
        public int CurrentLap { get; private set; }

        public readonly HashSet<int> PassedCheckpoints = new HashSet<int>();
        private float currentLapStartTime;
        public readonly Dictionary<int, (float time, bool valid)> LapTimes = new Dictionary<int, (float, bool)>();


        public void StartLapTimer(float startTime) => currentLapStartTime = startTime;

        public void CheckpointPassed(int index)
        {
            LatestCheckpoint = index;
            PassedCheckpoints.Add(index);
        }

        public bool TryCompleteLap(int checkpointAmount, float lapEndTime, bool countInvalidLap, out int lapCount)
        {
            bool lapIsValid = IsLapValid(checkpointAmount);

            if(lapIsValid || countInvalidLap)
            {
                LapTimes.Add(CurrentLap, (lapEndTime - currentLapStartTime, lapIsValid));
                CurrentLap++;
            }

            StartLapTimer(lapEndTime);
            PassedCheckpoints.Clear();
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