using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/TyreData")]
    public class TyreData : ScriptableObject
    {
        [Header("Base Tyre Data")]
        [Min(0)]
        public float Mass = 20f;
        [Min(float.Epsilon), Tooltip("Tyre radius in mm")]
        public float Radius = 300f;
        [Range(0f, 20f)]
        public float CompoundBaseFriction = 3f;
        [Range(float.Epsilon, 0.1f)]
        public float RollingResistance = 0.01f;
        [Range(0f, 2f)]
        public float SidewaysFrictionCoefficient = 1f;
        [Range(0f, 2f)]
        public float ForwardsFrictionCoefficient = 1.4f;

        //[Header("Slip Data")]
        //[Range(0f, 1f), Tooltip("Grip at locked brakes or at 90 degree drift (Higher = More grip when sliding)")]
        //public float MinGripDuringSlip;
        //[Range(0f, 90f), Tooltip("Drift angle where tyres start losing grip (Higher = More grip in corners)")]
        //public float PeakGripMaxAngle = 20f;

        [Header("Friction calculation data")]
        [SerializeField, Range(0f, 2f)]
        private float _stationaryFrictionCoefficient = 0.95f;
        [SerializeField, Min(0)]
        private float _stationaryFrictionMaxVelocity = 0.5f;

        [SerializeField, Space, Range(0f, 2f)]
        private float _peakFrictionCoefficient = 1.2f;
        [SerializeField, Min(0)]
        private float _peakFrictionVelocity = 1f;

        [SerializeField, Space, Range(0f, 2f)]
        private float _limitFrictionCoefficient = 0.5f;
        [SerializeField, Min(0)]
        private float _limitFrictionMinVelocity = 5f;
        
        [Header("Friction Curve (Read only)")]
        [SerializeField, Space]
        private AnimationCurve _frictionCurve;

        [ContextMenu("InitCurve")]
        public void InitFrictionCurve()
        {
            Keyframe stationary = new Keyframe(_stationaryFrictionMaxVelocity, _stationaryFrictionCoefficient, 0f, 0f, 0f, 0f);
            Keyframe peak = new Keyframe(_peakFrictionVelocity, _peakFrictionCoefficient, 0f, 0f, 0.6f, 0.2f);
            Keyframe limit = new Keyframe(_limitFrictionMinVelocity, _limitFrictionCoefficient, 0f, 0f, 0.8f, 0f);

            stationary.weightedMode = WeightedMode.Out;
            peak.weightedMode = WeightedMode.Both;
            limit.weightedMode = WeightedMode.In;

            _frictionCurve = new AnimationCurve(stationary, peak, limit);
        }

        public float GetFrictionCoefficientAtSpeed(float speed) => _frictionCurve.Evaluate(speed);
        public float MaxFrictionCoefficient() => Mathf.Max(_stationaryFrictionCoefficient, _peakFrictionCoefficient, _limitFrictionCoefficient);
        public float MinFrictionCoefficient() => Mathf.Min(_stationaryFrictionCoefficient, _peakFrictionCoefficient, _limitFrictionCoefficient);
    }
}