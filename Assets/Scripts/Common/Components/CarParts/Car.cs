using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.CarParts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Car : MonoBehaviour
    {
        private const float MS_TO_KMH_CONVERSION = 3.6f;

        private delegate void CarDataChangedEvent();
        private event CarDataChangedEvent CarDataChanged;
        private event CarDataChangedEvent CarDataCustomizationChanged;

        [SerializeField]
        private CarData _carData = null;
        [SerializeField]
        private CarCustomizationData _customizationData = null;
        [SerializeField]
        private Axle _frontAxle = null;
        [SerializeField]
        private Axle _rearAxle = null;
        [SerializeField]
        private Engine _engine = null;
        [SerializeField]
        private Drivetrain _drivetrain = null;
        [SerializeField]
        private Body _body = null;

        private Rigidbody2D _rigidbody;
        private Vector2 _currentPosition => new Vector2(transform.position.x, transform.position.y);

        private float _currentBodyMass;
        private Color _currentBaseColor;
        private Color _currentCustomizationColor1;
        private Color _currentCustomizationColor2;

        public float CurrentForceMomentum => _carData.CarTotalMass * (CurrentVelocityInMetersPerSecond / Time.fixedDeltaTime);

        public float CurrentVelocityInMetersPerSecond => _rigidbody.velocity.magnitude;
        public float CurrentVelocityInKilometersPerHour => CurrentVelocityInMetersPerSecond * MS_TO_KMH_CONVERSION;

        public float CurrentForwardsVelocityInMetersPerSecond => _rigidbody.velocity.y;
        public float CurrentForwardVelocityInKilometersPerHour => CurrentForwardsVelocityInMetersPerSecond * MS_TO_KMH_CONVERSION;

        public Vector3 CurrentPosition => _body.transform.position;
        public float CurrentVelocityDirection => Vector2.SignedAngle(Vector2.up, _rigidbody.velocity.normalized);
        public float CurrentVehicleRotation => transform.rotation.eulerAngles.z;

        private float _acceleration = 0f;
        private float _previousFrameVelocity = 0f;

        private void Awake()
        {
            Init(_carData);
        }

        private void Update()
        {
            CheckExternalCarDataChanges();
            CheckCustomizationDataChanges();
        }

        private void FixedUpdate()
        {
            _acceleration = (CurrentVelocityInMetersPerSecond - _previousFrameVelocity) / Time.fixedDeltaTime;
            _previousFrameVelocity = CurrentVelocityInMetersPerSecond;
        }

        private void Init(CarData carData)
        {
            carData.FrontTyres.InitFrictionCurve();
            carData.RearTyres.InitFrictionCurve();

            _rigidbody = GetComponent<Rigidbody2D>();

            _engine.Init(_drivetrain, carData);
            _frontAxle.Init(carData, SteeringTypeIdentifier.FRONT);
            _rearAxle.Init(carData, SteeringTypeIdentifier.REAR);
            _drivetrain.Init(_frontAxle, _rearAxle, carData);
            _body.Init(_customizationData);

            UpdateCarMassData();
            CarDataChanged += OnCarDataChanged;
            CarDataCustomizationChanged += OnCarCustomizationDataChanged;
        }

        private void UpdateCarMassData()
        {
            _rigidbody.mass = _carData.VehicleMass;
            _currentBodyMass = _carData.VehicleMass;
        }

        private void UpdateCarCustomizationData()
        {
            _body.SetCustomization();

            _currentBaseColor = _customizationData.BaseColor;
            _currentCustomizationColor1 = _customizationData.Customization1;
            _currentCustomizationColor2 = _customizationData.Customization2;
        }

        public void Accelerate(float strength) => _engine.Accelerate(strength);
        public void Brake(float strength) => _frontAxle.Brake(strength);
        public void Handbrake(float strength) => _rearAxle.Brake(strength);

        public void Turn(float strength)
        {
            _frontAxle.Turn(strength);
            _rearAxle.Turn(strength);
        }

        private void CheckExternalCarDataChanges()
        {
            if(_currentBodyMass != _carData.VehicleMass)
            {
                CarDataChanged?.Invoke();
            }
        }

        private void CheckCustomizationDataChanges()
        {
            if(_currentBaseColor != _customizationData.BaseColor ||
            _currentCustomizationColor1 != _customizationData.Customization1 ||
            _currentCustomizationColor2 != _customizationData.Customization2)
            {
                CarDataCustomizationChanged?.Invoke();
            }
        }

        private void OnCarDataChanged() => UpdateCarMassData();
        private void OnCarCustomizationDataChanged() => UpdateCarCustomizationData();

        private void OnDrawGizmos()
        {
            if(_rigidbody == null)
            {
                return;
            }

            Gizmos.DrawLine(_currentPosition, _currentPosition + _rigidbody.velocity);
            //Debug.Log($"Current Speed: {CurrentVelocityInKilometersPerHour} KM/H");
            //Debug.Log($"Current acceleration {acceleration} ms^2");
        }
    }
}