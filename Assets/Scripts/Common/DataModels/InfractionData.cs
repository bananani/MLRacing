using Common.Identifiers;
using UnityEngine;

namespace Common.DataModels
{
    public struct InfractionData
    {
        public int CheckpointIndex { get; }
        public float TimeOfInfraction { get; }
        public Vector2 CarPosition { get; }
        public float Speed { get; }
        public float Acceleration { get; }
        public InfractionSeverityIdentifier Severity { get; }

        public InfractionData(int checkpointIndex, float timeOfInfraction, Vector2 carPosition, float speed, float acceleration, InfractionSeverityIdentifier severity)
        {
            CheckpointIndex = checkpointIndex;
            TimeOfInfraction = timeOfInfraction;
            CarPosition = carPosition;
            Speed = speed;
            Acceleration = acceleration;
            Severity = severity;
        }
    }
}