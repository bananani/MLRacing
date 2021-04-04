using Common.Constants;
using Common.DataModels.Debug;
using Common.Identifiers;
using Common.ScriptableObjects;
using Common.Utils;
using UnityEngine;

namespace Common.Components.CarParts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Car : MonoBehaviour
    {
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

        private Transponder _transponder;

        private Rigidbody2D _rigidbody;
        private Vector2 _currentPosition => new Vector2(transform.position.x, transform.position.y);

        private float _currentBodyMass;
        private Color _currentBaseColor;
        private Color _currentCustomizationColor1;
        private Color _currentCustomizationColor2;

        public float CurrentForceMomentum => _carData.CarTotalMass * (CurrentVelocityInMetersPerSecond / Time.fixedDeltaTime);

        public float CurrentVelocityInMetersPerSecond => _rigidbody.velocity.magnitude;
        public float CurrentVelocityInKilometersPerHour => CurrentVelocityInMetersPerSecond * CVelocity.MS_TO_KMH_CONVERSION;

        public float CurrentForwardsVelocityInMetersPerSecond => _relativeVelocity.y;
        public float CurrentForwardVelocityInKilometersPerHour => CurrentForwardsVelocityInMetersPerSecond * CVelocity.MS_TO_KMH_CONVERSION;

        public Vector3 CurrentPosition => _body.transform.position;
        public float CurrentVelocityDirection => Vector2.SignedAngle(Vector2.up, _rigidbody.velocity.normalized);
        public float CurrentVehicleRotation => transform.rotation.eulerAngles.z;

        private Vector2 _relativeVelocity => Vector2Utils.GetRotatedVelocityVector(_rigidbody.velocity, -transform.rotation.eulerAngles.z);
        private Vector2 _acceleration;
        private Vector2 _previousFrameVelocity;

        private float _currentDriftAngle => Mathf.Min(Vector2.Angle(Vector2.up, _relativeVelocity), Vector2.Angle(Vector2.up, -_relativeVelocity));

        public void Awake()
        {
            Init(_carData);
        }

        public void Update()
        {
            CheckExternalCarDataChanges();
            CheckCustomizationDataChanges();
        }

        public void FixedUpdate()
        {
            _acceleration = (_relativeVelocity - _previousFrameVelocity) / Time.fixedDeltaTime;
            _previousFrameVelocity = _relativeVelocity;

            ApplyDownforce();
            ApplyAirResistance();
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

        private void UpdateCarCustomizationData() => SetCarCustomizationData(_customizationData);

        private void SetCarCustomizationData(CarCustomizationData customizationData)
        {
            _customizationData = customizationData;
            _body.SetCustomization(_customizationData);

            _currentBaseColor = _customizationData.BaseColor;
            _currentCustomizationColor1 = _customizationData.Customization1;
            _currentCustomizationColor2 = _customizationData.Customization2;
        }

        public void SetupCarForDriver(DriverData driverData) => SetCarCustomizationData(driverData.PersonalCarCustomizationSettings);
        public Transponder SetupTransponder()
        {
            if(_transponder == null)
            {
                _transponder = GetComponentInChildren<Transponder>();
                if(_transponder == null)
                {
                    _transponder = _body.gameObject.AddComponent<Transponder>();
                    _transponder.Init();
                }
            }

            return _transponder;
        }
        public void Accelerate(float strength) => _engine.Accelerate(strength);
        public void Brake(float strength) => _frontAxle.Brake(strength);
        public void Handbrake(float strength) => _rearAxle.Brake(strength);

        public void Turn(float strength)
        {
            _frontAxle.Turn(strength);
            _rearAxle.Turn(strength);
        }

        public void ApplyAirResistance() => _rigidbody.AddForce(-_rigidbody.velocity.normalized * _body.GetAirResistance(CurrentVelocityInMetersPerSecond, _currentDriftAngle));

        public void ApplyDownforce()
        {
            float forwardsVelocity = Mathf.Max(0f, CurrentForwardsVelocityInMetersPerSecond);
            float downforceMultiplier = CAero.AIR_DENSITY * (forwardsVelocity * forwardsVelocity) * 0.5f;

            _frontAxle.ApplyDownforce(_carData.BodyKit.FrontSplitterSurfaceArea * _carData.BodyKit.FrontSplitterLiftCoefficient * downforceMultiplier);
            _rearAxle.ApplyDownforce(_carData.BodyKit.RearWingSurfaceArea * _carData.BodyKit.RearWingLiftCoefficient * downforceMultiplier);
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

        public CarDebugData CollectDebugData()
        {
            (TyreDebugData leftFrontTyreDebugData, TyreDebugData rightFrontTyreDebugData) = _frontAxle.CollectDebugData();
            (TyreDebugData leftRearTyreDebugData, TyreDebugData rightRearTyreDebugData) = _rearAxle.CollectDebugData();
            AeroDebugData aeroDebugData = new AeroDebugData(_frontAxle.CurrentDownforce, _rearAxle.CurrentDownforce, _body.GetAirResistance(CurrentVelocityInMetersPerSecond, _currentDriftAngle));
            EngineDebugData engineDebugData = _engine.CollectDebugData();
            VelocityDebugData velocityDebugData = new VelocityDebugData(_relativeVelocity, _acceleration, _currentDriftAngle);

            return new CarDebugData(
                velocityDebugData,
                engineDebugData,
                leftFrontTyreDebugData,
                rightFrontTyreDebugData,
                leftRearTyreDebugData,
                rightRearTyreDebugData,
                aeroDebugData
            );
        }

        private void OnCarDataChanged() => UpdateCarMassData();
        private void OnCarCustomizationDataChanged() => UpdateCarCustomizationData();

        public void PlaceOnGrid(Transform gridPlacement)
        {
            if(gridPlacement == null)
            {
                return;
            }

            _frontAxle.DisableTyres();
            _rearAxle.DisableTyres();

            _rigidbody.velocity = Vector2.zero;
            _rigidbody.angularVelocity = 0f;

            float offsetDistance = (_body.BodyLength / 2f) + 1f;
            float rotation = gridPlacement.rotation.eulerAngles.z;
            Vector2 offset = Vector2Utils.GetRotatedVelocityVector(new Vector2(0f, -offsetDistance), rotation);
            transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            transform.position = new Vector2(gridPlacement.position.x, gridPlacement.position.y) + offset;

            _frontAxle.EnableTyres();
            _rearAxle.EnableTyres();
        }

        public void OnDrawGizmos()
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