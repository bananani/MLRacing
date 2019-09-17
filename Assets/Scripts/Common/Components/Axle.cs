using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
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
        private bool _reverseSteering => (_steeringType & SteeringTypeIdentifier.REAR) == SteeringTypeIdentifier.REAR;

        public void Init(CarData carData, SteeringTypeIdentifier steeringType)
        {
            _carData = carData;
            _steeringType = steeringType;
            _leftTyre.Init(carData, _reverseSteering ? TyreIdentifier.RL : TyreIdentifier.FL);
            _rightTyre.Init(carData, _reverseSteering ? TyreIdentifier.RR : TyreIdentifier.FR);
        }

        public void SetTyreMass(float mass)
        {
            _leftTyre.SetTyreMass(mass);
            _rightTyre.SetTyreMass(mass);
        }

        public void Accelerate(float torque)
        {
            _leftTyre.AddForce(torque);
            _rightTyre.AddForce(torque);
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

            float turnDegrees = _reverseSteering ? -(_maxTurnRadius * turningStrength) : (_maxTurnRadius * turningStrength);

            _leftTyre.Turn(turnDegrees);
            _rightTyre.Turn(turnDegrees);
        }
    }
}