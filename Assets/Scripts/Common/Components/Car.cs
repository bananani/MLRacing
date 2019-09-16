using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Car : MonoBehaviour
    {
        [SerializeField]
        private CarData _carData = null;
        [SerializeField]
        private Axle _frontAxle = null;
        [SerializeField]
        private Axle _rearAxle = null;
        [SerializeField]
        private Engine _engine = null;
        [SerializeField]
        private Drivetrain _drivetrain = null;

        private Rigidbody2D _rigidbody;
        private Vector2 _currentPosition => new Vector2(transform.position.x, transform.position.y);

        private void Awake()
        {
            Init(_carData);
        }

        private void Init(CarData carData)
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _engine.Init(_drivetrain, carData);
            _frontAxle.Init(carData, SteeringTypeIdentifier.FRONT);
            _rearAxle.Init(carData, SteeringTypeIdentifier.REAR);
            _drivetrain.Init(_frontAxle, _rearAxle, carData);
        }

        public void Accelerate(float strength)
        {
            _engine.Accelerate(strength);
        }

        public void Brake(float strength)
        {
            _frontAxle.Brake(strength);
        }

        public void Handbrake(float strength)
        {
            _rearAxle.Brake(strength);
        }

        public void Turn(float strength)
        {
            _frontAxle.Turn(strength);
            _rearAxle.Turn(strength);
        }

        private void OnDrawGizmos()
        {
            if(_rigidbody == null)
            {
                return;
            }

            Gizmos.DrawLine(_currentPosition, _currentPosition + _rigidbody.velocity);
        }
    }
}