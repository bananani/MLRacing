using Common.Components;
using Common.Constants;
using Common.DataModels.Debug;
using System;
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

        private void OnEnable() => DebugDataObject.DebugDataCollected += OnDebugDataCollected;
        private void OnDisable() => DebugDataObject.DebugDataCollected -= OnDebugDataCollected;

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
            _currentVelocityLabel?.SetText($"Velocity: {speed.ToString("0.0")} m/s ({(speed * CVelocity.MS_TO_KMH_CONVERSION).ToString("0.0")} kmh)");
            _currentAccelerationLabel?.SetText($"Acceleration: {data.Acceleration}");
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
            container?.SurfaceLabel?.SetText("Surfc: " + data.Surface);
            container?.AccelerationForceLabel?.SetText("Accel: " + data.AccelerationForce.ToString());
            container?.CurrentVelocityLabel?.SetText("CVel: " + data.CurrentRelativeVelocity.ToString());
            container?.SidewaysFrictionLabel?.SetText("SideF: " + data.SidewaysFriction.ToString());
            container?.RollingFrictionLabel?.SetText("RollF: " + data.RollingFriction.ToString());
            container?.BrakingFrictionLabel?.SetText("StopF: " + data.BrakingFriction.ToString());
            container?.MassResponsibilityLabel?.SetText("MassR: " + data.MassResponsibility.ToString());
            container?.EffectiveGripLabel?.SetText("EGrip: " + data.EffectiveGrip.ToString());
            container?.DownforceLabel?.SetText("DownF: " + data.Downforce.ToString());
            container?.TotalGripLabel?.SetText("Total: " + data.TotalGrip.ToString());
        }
    }
}