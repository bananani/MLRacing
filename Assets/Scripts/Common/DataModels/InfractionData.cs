using Common.Identifiers;

namespace Common.DataModels
{
    public struct InfractionData
    {
        public int CheckpointIndex { get; }
        public float TimeOfInfraction { get; }
        public float Speed { get; }
        public float Acceleration { get; }
        public InfractionSeverityIdentifier Severity { get; }

        public InfractionData(int checkpointIndex, float timeOfInfraction, float speed, float acceleration, InfractionSeverityIdentifier severity)
        {
            CheckpointIndex = checkpointIndex;
            TimeOfInfraction = timeOfInfraction;
            Speed = speed;
            Acceleration = acceleration;
            Severity = severity;
        }
    }
}