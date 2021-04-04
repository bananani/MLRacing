using Common.Components.Debug;
using Common.Constants;
using Common.DataModels.Debug;
using TMPro;
using UnityEngine;

namespace Common.UserInterface
{
    public class DebugView : MonoBehaviour
    {
        [Header("Velocity labels")]
        [SerializeField]
        private TextMeshProUGUI _currentVelocityLabel;
        [SerializeField]
        private TextMeshProUGUI _currentAccelerationLabel;
        [SerializeField]
        private TextMeshProUGUI _currentDriftAngleLabel;

        [Header("Aero labels")]
        [SerializeField]
        private TextMeshProUGUI _frontAeroDownforceLabel;
        [SerializeField]
        private TextMeshProUGUI _rearAeroDownforceLabel;
        [SerializeField]
        private TextMeshProUGUI _airResistanceLabel;

        [Header("Tyre labels")]
        [SerializeField]
        private TyreDebugDataContainer _leftFrontTyre;
        [SerializeField]
        private TyreDebugDataContainer _rightFrontTyre;
        [SerializeField]
        private TyreDebugDataContainer _leftRearTyre;
        [SerializeField]
        private TyreDebugDataContainer _rightRearTyre;

        public void OnEnable() => DebugDataObject.DebugDataCollected += OnDebugDataCollected;
        public void OnDisable() => DebugDataObject.DebugDataCollected -= OnDebugDataCollected;

        private void OnDebugDataCollected(DebugData debugData)
        {
            SetVelocityDataLabels(debugData.Car.Velocity);
            SetEngineDataLabels(debugData.Car.Engine);
            SetAeroDataLabels(debugData.Car.Aero);

            SetTyreDataLabels(_leftFrontTyre, debugData.Car.LeftFrontTyre);
            SetTyreDataLabels(_rightFrontTyre, debugData.Car.RightFrontTyre);
            SetTyreDataLabels(_leftRearTyre, debugData.Car.LeftRearTyre);
            SetTyreDataLabels(_rightRearTyre, debugData.Car.RightRearTyre);
        }

        private void SetVelocityDataLabels(VelocityDebugData data)
        {
            float speed = data.Velocity.magnitude;
            _currentVelocityLabel?.SetText($"Velocity: {speed:0.0} m/s ({speed * CVelocity.MS_TO_KMH_CONVERSION:0.0} kmh)");
            _currentAccelerationLabel?.SetText($"Acceleration: {data.Acceleration}");
            _currentDriftAngleLabel?.SetText($"Drift Angle: {data.DriftAngle}°");
        }

        private void SetEngineDataLabels(EngineDebugData engine) { }

        private void SetAeroDataLabels(AeroDebugData aero)
        {
            _frontAeroDownforceLabel?.SetText($"Downforce Front: {aero.FrontSplitterDownforce}");
            _rearAeroDownforceLabel?.SetText($"Downforce Rear: {aero.RearWingDownforce}");
            _airResistanceLabel?.SetText($"AirResistance: {aero.CurrentAirResistance}");
        }

        private void SetTyreDataLabels(TyreDebugDataContainer container, TyreDebugData data)
        {
            container?.SurfaceLabel?.SetText($"Surfc: {data.Surface}");
            container?.AccelerationForceLabel?.SetText($"Accel: {data.AccelerationForce}");
            container?.CurrentVelocityLabel?.SetText($"CVel: {data.CurrentRelativeVelocity}");
            container?.SidewaysFrictionLabel?.SetText($"SideF: {data.SidewaysFriction.x} N");
            container?.RollingFrictionLabel?.SetText($"RollF: {data.RollingFriction.y} N");
            container?.BrakingFrictionLabel?.SetText($"StopF: {data.BrakingFriction.y} N");
            container?.MassResponsibilityLabel?.SetText($"MassR: {data.MassResponsibility} N");
            container?.EffectiveGripLabel?.SetText($"EGrip: {data.EffectiveGrip}");
            container?.DownforceLabel?.SetText($"DownF: {data.Downforce}");
            container?.TotalGripLabel?.SetText($"Total: {data.TotalGrip}");
        }
    }
}