using UnityEngine;

namespace Common.DataModels.Debug
{
    public struct TyreDebugData
    {
        public Vector2 CurrentRelativeVelocity { get; }
        public Vector2 AccelerationForce { get; }

        public Vector2 SidewaysFriction { get; }
        public Vector2 RollingFriction { get; }
        public Vector2 BrakingFriction { get; }

        public float MassResponsibility { get; }
        public float EffectiveGrip { get; }
        public float Downforce { get; }
        public float TotalGrip { get; }

        public string Surface { get; }

        public TyreDebugData(
            Vector2 currentRelativeVelocity, 
            Vector2 accelerationForce, 
            Vector2 sidewaysFriction, 
            Vector2 rollingFriction, 
            Vector2 brakingFriction, 
            float massResponsibility, 
            float effectiveGrip, 
            float downforce, 
            float totalGrip, 
            string surface) : this()
        {
            CurrentRelativeVelocity = currentRelativeVelocity;
            AccelerationForce = accelerationForce;
            SidewaysFriction = sidewaysFriction;
            RollingFriction = rollingFriction;
            BrakingFriction = brakingFriction;
            MassResponsibility = massResponsibility;
            EffectiveGrip = effectiveGrip;
            Downforce = downforce;
            TotalGrip = totalGrip;
            Surface = surface;
        }
    }
}