using Common.Components;
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

        [Header("Aero labels")]
        [SerializeField]
        private TextMeshProUGUI _frontAeroDownforceLabel;
        [SerializeField]
        private TextMeshProUGUI _rearAeroDownforceLabel;

        [Header("LeftFrontTyre labels")]
        [SerializeField]
        private TextMeshProUGUI _lfSurfaceLabel;

        [Header("RightFrontTyre labels")]
        [SerializeField]
        private TextMeshProUGUI _rfSurfaceLabel;

        [Header("LeftRearTyre labels")]
        [SerializeField]
        private TextMeshProUGUI _lrSurfaceLabel;

        [Header("RightRearTyre labels")]
        [SerializeField]
        private TextMeshProUGUI _rrSurfaceLabel;

        private void OnEnable() => DebugDataObject.DebugDataCollected += OnDebugDataCollected;
        private void OnDisable() => DebugDataObject.DebugDataCollected -= OnDebugDataCollected;

        private void OnDebugDataCollected(DebugData debugData)
        {
            SetVelocityDataLabels(debugData.Car.Velocity);
            SetEngineDataLabels(debugData.Car.Engine);
            SetAeroDataLabels(debugData.Car.Aero);
            SetLeftFrontDataLabels(debugData.Car.LeftFrontTyre);
            SetRightFrontDataLabels(debugData.Car.RightFrontTyre);
            SetLeftRearDataLabels(debugData.Car.LeftRearTyre);
            SetRightRearDataLabels(debugData.Car.RightRearTyre);
        }

        private void SetVelocityDataLabels(VelocityDebugData data)
        {
            _currentVelocityLabel?.SetText($"Velocity: {data.Velocity}");
            _currentAccelerationLabel?.SetText($"Acceleration: {data.Acceleration}");
        }

        private void SetEngineDataLabels(EngineDebugData engine) { }

        private void SetAeroDataLabels(AeroDebugData aero)
        {
            _frontAeroDownforceLabel?.SetText($"Downforce Front: {aero.FrontSplitterDownforce}");
            _rearAeroDownforceLabel?.SetText($"Downforce Rear: {aero.RearWingDownforce}");
        }

        private void SetLeftFrontDataLabels(TyreDebugData data)
        {
            _lfSurfaceLabel?.SetText(data.Surface);
        }

        private void SetRightFrontDataLabels(TyreDebugData data)
        {
            _rfSurfaceLabel?.SetText(data.Surface);
        }

        private void SetLeftRearDataLabels(TyreDebugData data)
        {
            _lrSurfaceLabel?.SetText(data.Surface);
        }

        private void SetRightRearDataLabels(TyreDebugData data)
        {
            _rrSurfaceLabel?.SetText(data.Surface);
        }
    }
}