using Common.DataModels.Debug;
using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.CarParts
{
    public class Axle : MonoBehaviour
    {
        private CarData _carData;

        [SerializeField]
        private Tyre _leftTyre = null;
        [SerializeField]
        private Tyre _rightTyre = null;

        private SteeringTypeIdentifier _steeringType;

        private float _maxTurnRadius => _carData.MaxSteeringAngle;
        private bool _useForSteering => (_carData.SteeringType & _steeringType) == _steeringType;
        private bool _isRearAxle => (_steeringType & SteeringTypeIdentifier.REAR) == SteeringTypeIdentifier.REAR;

        public float CurrentDownforce { get; private set; }

        public void Init(CarData carData, SteeringTypeIdentifier steeringType)
        {
            _carData = carData;
            _steeringType = steeringType;
            _leftTyre.Init(carData, _isRearAxle ? TyrePositionIdentifier.RL : TyrePositionIdentifier.FL);
            _rightTyre.Init(carData, _isRearAxle ? TyrePositionIdentifier.RR : TyrePositionIdentifier.FR);
        }

        public void Accelerate(float torque)
        {
            _leftTyre.Accelerate(torque);
            _rightTyre.Accelerate(torque);
        }

        public void Brake(float strength)
        {
            _leftTyre.Brake(strength);
            _rightTyre.Brake(strength);
        }

        public void Turn(float turningStrength)
        {
            if(!_useForSteering)
            {
                _leftTyre.Turn(0);
                _rightTyre.Turn(0);
                return;
            }

            float turnDegrees = _isRearAxle ? -(_maxTurnRadius * turningStrength) : (_maxTurnRadius * turningStrength);

            _leftTyre.Turn(turnDegrees);
            _rightTyre.Turn(turnDegrees);
        }

        public void ApplyDownforce(float downforce)
        {
            CurrentDownforce = downforce;
            _leftTyre.ApplyDownforce(downforce * 0.5f);
            _rightTyre.ApplyDownforce(downforce * 0.5f);
        }

        public (TyreDebugData leftTyreDebugData, TyreDebugData rightTyreDebugData) CollectDebugData()
        {
            return (_leftTyre.CollectDebugData(), _rightTyre.CollectDebugData());
        }
    }
}