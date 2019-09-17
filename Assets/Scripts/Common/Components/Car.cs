using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Car : MonoBehaviour
    {
        private const float MS_TO_KMH_CONVERSION = 3.6f;

        private delegate void CarDataChangedEvent();
        private event CarDataChangedEvent CarDataChanged;

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

        private float _previousBodyMass;
        private float _previousFrontTyreMass;
        private float _previousRearTyreMass;

        public float CurrentVelocityInMetersPerSecond => _rigidbody.velocity.magnitude;
        public float CurrentVelocityInKilometersPerHour => CurrentVelocityInMetersPerSecond * MS_TO_KMH_CONVERSION;

        public float CurrentForwardsVelocityInMetersPerSecond => _rigidbody.velocity.y;
        public float CurrentForwardVelocityInKilometersPerHour => CurrentForwardsVelocityInMetersPerSecond * MS_TO_KMH_CONVERSION;

        private float _previousFrameVelocity = 0f;

        private void Awake()
        {
            Init(_carData);
        }

        private void Update()
        {
            CheckExternalCarDataChanges();
        }

        private void FixedUpdate()
        {
            Debug.Log($"Current Speed: {CurrentVelocityInKilometersPerHour} KM/H");

            float acceleration = (CurrentVelocityInMetersPerSecond - _previousFrameVelocity) / Time.fixedDeltaTime;
            Debug.Log($"Current acceleration {acceleration} ms^2");
            _previousFrameVelocity = CurrentVelocityInMetersPerSecond;
        }

        private void Init(CarData carData)
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _engine.Init(_drivetrain, carData);
            _frontAxle.Init(carData, SteeringTypeIdentifier.FRONT);
            _rearAxle.Init(carData, SteeringTypeIdentifier.REAR);
            _drivetrain.Init(_frontAxle, _rearAxle, carData);

            UpdateCarMassData();
            CarDataChanged += OnCarDataChanged;
        }

        private void UpdateCarMassData()
        {
            _rigidbody.mass = _carData.VehicleMass;
            _frontAxle.SetTyreMass(_carData.FrontTyreMass);
            _rearAxle.SetTyreMass(_carData.RearTyreMass);

            _previousBodyMass = _carData.VehicleMass;
            _previousFrontTyreMass = _carData.FrontTyreMass;
            _previousRearTyreMass = _carData.RearTyreMass;
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

        private void CheckExternalCarDataChanges()
        {
            if(_previousBodyMass != _carData.VehicleMass || _previousFrontTyreMass != _carData.FrontTyreMass || _previousRearTyreMass != _carData.RearTyreMass)
            {
                CarDataChanged?.Invoke();
            }
        }

        private void OnCarDataChanged()
        {
            UpdateCarMassData();
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