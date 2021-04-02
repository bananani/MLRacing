using UnityEngine;

namespace Common.DataModels.Debug
{
    public struct VelocityDebugData
    {
        public Vector2 Velocity { get; }
        public Vector2 Acceleration { get; }
        public float DriftAngle { get; }

        public VelocityDebugData(Vector2 currentVelocity, Vector2 currentAcceleration, float driftAngle) : this()
        {
            Velocity = currentVelocity;
            Acceleration = currentAcceleration;
            DriftAngle = driftAngle;
        }
    }
}