using Common.Constants;
using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(TrailRenderer))]
    public class Tyre : MonoBehaviour
    {
        private static readonly Vector3 _forwards = new Vector2(0, 1);
        private CarData _carData;

        private Vector3 _originalPosition;
        private Rigidbody2D _rigidbody;
        private TrailRenderer _trailRenderer;

        private float _currentBrakingForce;
        private float _rawInputForce;

        private TyreIdentifier _tyreType;
        private float _grip => _carData.Grip;
        private bool _accelerating = false;
        private float _currentSlipMultiplier = 1f;

        private Vector2 _relativeVelocity => GetRotatedVelocityVector(_rigidbody.velocity, -transform.eulerAngles.z);

        private float _currentTorque => (_rawInputForce * _currentSlipMultiplier * (1f - _currentBrakingForce)) / _carData.TyreRadius;
        private float _gripWithSlip => _grip * _currentSlipMultiplier;
        private Vector2 _sidewaysFrictionForce => new Vector2(-_relativeVelocity.x, 0) * _gripWithSlip;
        private Vector2 _rollingFrictionForce => new Vector2(0, -_relativeVelocity.y * CTyre.ROLLING_RESISTANCE);
        private Vector2 _brakingFrictionForce => new Vector2(0, -_relativeVelocity.y) * _currentBrakingForce * _gripWithSlip;

        private void Start()
        {
            _originalPosition = transform.localPosition;
            _rigidbody = GetComponent<Rigidbody2D>();
            _trailRenderer = GetComponent<TrailRenderer>();
        }

        public void Init(CarData carData, TyreIdentifier tyreType)
        {
            _carData = carData;
            _tyreType = tyreType;
        }

        private void Update()
        {
            HoldOriginalPosition();
        }

        private void FixedUpdate()
        {
            CalculateSlip();
            AddSidewaysGrip();
            AddRollingFriction();
        }

        public void AddForce(float force)
        {
            _rawInputForce = force;
            _accelerating = _rawInputForce != 0;
            _rigidbody.AddRelativeForce(_forwards * _currentTorque);
        }

        public void Brake(float strength)
        {
            _currentBrakingForce = strength;
            _rigidbody.AddRelativeForce(_brakingFrictionForce);
        }

        public void Turn(float degrees)
        {
            transform.localEulerAngles = new Vector3(0, 0, GetToeAngle() + degrees);
        }

        private void AddRollingFriction()
        {
            if(_accelerating)
            {
                return;
            }

            _rigidbody.AddRelativeForce(_rollingFrictionForce * _grip);
        }

        private void AddSidewaysGrip()
        {
            _rigidbody.AddRelativeForce(_sidewaysFrictionForce);
        }

        private void CalculateSlip()
        {
            _currentSlipMultiplier = GetCurrentSlipMultiplier();

            if(_currentBrakingForce >= 1f)
            {
                _currentSlipMultiplier = _carData.MinGripDuringSlip;
            }

            _trailRenderer.emitting = _currentSlipMultiplier < CTyre.TYRE_TRAIL_SLIP_THRESHOLD;
        }

        private float GetToeAngle()
        {
            switch(_tyreType)
            {
                case TyreIdentifier.FL:
                    return -_carData.ToeFront;
                case TyreIdentifier.FR:
                    return _carData.ToeFront;
                case TyreIdentifier.RL:
                    return -_carData.ToeRear;
                case TyreIdentifier.RR:
                    return _carData.ToeRear;
                default:
                    return 0;
            }
        }

        private void HoldOriginalPosition()
        {
            transform.localPosition = _originalPosition;
        }

        private static Vector2 GetRotatedVelocityVector(Vector2 original, float degOfRotation)
        {
            Vector2 velocity = original;
            float rotation = Mathf.Deg2Rad * degOfRotation;
            float cos = Mathf.Cos(rotation);
            float sin = Mathf.Sin(rotation);

            float x = (velocity.x * cos) - (velocity.y * sin);
            float y = (velocity.x * sin) + (velocity.y * cos);

            return new Vector2(x, y);
        }

        public float GetDriftAngle()
        {
            float forwards = Vector2.Angle(_rigidbody.velocity, GetRotatedVelocityVector(_forwards, transform.rotation.eulerAngles.z));
            float backwards = Vector2.Angle(_rigidbody.velocity, GetRotatedVelocityVector(-_forwards, transform.rotation.eulerAngles.z));
            return Mathf.Min(forwards, backwards);
        }

        private float GetCurrentSlipMultiplier()
        {
            float currentDriftAngle = GetDriftAngle();
            if(currentDriftAngle < _carData.MinGripLossAngle)
            {
                return 1f;
            }

            // Comment assumptions
            //   40 degree drift angle
            //   min grip @ 90 degrees: 30%
            //   max grip @ <20%: 100%

            // Angle of drift relative to the lowest angle ( 40 - 20 = 20 degrees )
            float driftDeltaAngle = currentDriftAngle - _carData.MinGripLossAngle;
            // How much distance between min and max angle ( 90 - 20 = 70 degrees )
            float driftAngleVariation = CTyre.MAX_DRIFT_ANGLE - _carData.MinGripLossAngle;
            // How many percent is driftDeltaAngle from driftAngleVariation ( 20 / 70 = 0.286 )
            float driftAnglePercent = driftDeltaAngle / driftAngleVariation;

            // Invert the drift angle, we want to make it stand for grip and more is better ( 1 - 0.286 = 0.714)
            float invertedDriftAnglePercent = 1f - driftAnglePercent;

            // Calculate the distance between min and max grip ( 1 - 0.3 = 0.7 )
            float slipAmountVariation = CTyre.MAX_GRIP - _carData.MinGripDuringSlip;
            // Calculate the grip amount within the variation and re-add the minimum we reduced in the last step
            // ( ( 0.714 * 0.7 ) + 0.3 = 0.8 )
            float slipMultiplier = (invertedDriftAnglePercent * slipAmountVariation) + _carData.MinGripDuringSlip;

            // Return the grip multiplier ( 80% )
            return slipMultiplier;
        }

#if UNITY_EDITOR
        private const float GIZMO_SCALE = 0.001f;
        private void OnDrawGizmos()
        {
            if(_rigidbody == null)
            {
                return;
            }

            Vector2 locationIn2DSpace = new Vector2(transform.position.x, transform.position.y);

            Vector2 torqueForce = _forwards * _currentTorque;

            Vector2 correctedRollingFrictionForce = GetRotatedVelocityVector(_rollingFrictionForce, transform.rotation.eulerAngles.z) * GIZMO_SCALE;
            Vector2 correctedTorque = GetRotatedVelocityVector(torqueForce, transform.rotation.eulerAngles.z) * GIZMO_SCALE;
            Vector2 correctedSidewaysFrictionForce = GetRotatedVelocityVector(_sidewaysFrictionForce, transform.rotation.eulerAngles.z) * GIZMO_SCALE;
            Vector2 correctedBrakingFrictionForce = GetRotatedVelocityVector(_brakingFrictionForce, transform.rotation.eulerAngles.z) * GIZMO_SCALE;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedRollingFrictionForce);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedBrakingFrictionForce);

            Gizmos.color = GetCurrentSlipMultiplier() < 1f ? Color.red : Color.green;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedTorque);
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedSidewaysFrictionForce);
        }
#endif
    }
}