using UnityEngine;

namespace Common.DataModels.Debug
{
    public struct VelocityDebugData
    {
        public Vector2 Velocity { get; }
        public Vector2 Acceleration { get; }

        public VelocityDebugData(Vector2 currentVelocity, Vector2 currentAcceleration) : this()
        {
            Velocity = currentVelocity;
            Acceleration = currentAcceleration;
        }
    }
}