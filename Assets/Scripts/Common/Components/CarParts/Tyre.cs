using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.CarParts
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(TrailRenderer))]
    public class Tyre : MonoBehaviour
    {
        private static readonly Vector3 _forwards = new Vector2(0, 1);
        private CarData _carData;

        private Rigidbody2D _rigidbodyReference;
        private Rigidbody2D _rigidbody
        {
            get
            {
                if(_rigidbodyReference == null)
                {
                    _rigidbodyReference = GetComponent<Rigidbody2D>();
                }

                return _rigidbodyReference;
            }
        }

        private Vector3 _originalPosition;
        private TrailRenderer _trailRenderer;
        private ParticleSystem _smokeSystem;

        private float _currentBrakingForce;
        private float _rawInputForce;

        private TyreIdentifier _tyreType;
        private float _grip => _carData.Grip;
        private bool _accelerating = false;
        private SurfaceMaterial _currentSurface;
        private float _surfaceSidewaysGripCoefficient => _currentSurface?.SidewaysGripCoefficient ?? 1f;
        private float _surfaceForwardsGripCoefficient => _currentSurface?.ForwardsGripCoefficient ?? 1f;
        private float _currentSlipMultiplier = 1f;
        private float _sidewaysGripWithSlip => _surfaceSidewaysGripCoefficient * _currentSlipMultiplier * _grip;


        private Vector2 _relativeVelocity => GetRotatedVelocityVector(_rigidbody.velocity, -transform.eulerAngles.z);

        private float _currentTorque => (_rawInputForce * Mathf.Min(1f, _currentSlipMultiplier * 2.5f) * (1f - _currentBrakingForce)) / _carData.TyreRadius;
        private Vector2 _sidewaysFrictionForce => new Vector2(-_relativeVelocity.x, 0) * _sidewaysGripWithSlip;
        private Vector2 _rollingFrictionForce => new Vector2(0, -_relativeVelocity.y * CTyre.ROLLING_RESISTANCE);
        private Vector2 _brakingFrictionForce => new Vector2(0, -_relativeVelocity.y) * _currentBrakingForce * _sidewaysGripWithSlip;


        private Ray _gripRay => new Ray(transform.position, Vector3.forward * 1000f);

        private void Start()
        {
            _originalPosition = transform.localPosition;
            _trailRenderer = GetComponent<TrailRenderer>();
            _smokeSystem = GetComponent<ParticleSystem>();
        }

        public void Init(CarData carData, TyreIdentifier tyreType)
        {
            _carData = carData;
            _tyreType = tyreType;
        }

        private void Update()
        {
            HoldOriginalPosition();
            _currentSurface = GetSurface();
        }

        private void FixedUpdate()
        {
            CalculateSlip();
            AddSidewaysGrip();
            AddRollingFriction();
        }

        public void SetTyreMass(float mass)
        {
            _rigidbody.mass = mass;
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
            ParticleSystem.EmissionModule emission = _smokeSystem.emission;
            emission.enabled = _currentSlipMultiplier < CTyre.TYRE_TRAIL_SLIP_THRESHOLD;
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

        private SurfaceMaterial GetSurface()
        {
            RaycastHit[] result = Physics.RaycastAll(_gripRay, float.MaxValue);

            for(int i = result.Length - 1; i >= 0; i--)
            {
                RaycastHit hit = result[i];
                GameObject go = hit.transform.gameObject;
                SpriteRenderer rend = go.GetComponent<SpriteRenderer>();
                if(rend == null)
                {
                    continue;
                }

                Vector2 localPos = hit.transform.InverseTransformPoint(hit.point);
                localPos += rend.sprite.pivot / rend.sprite.pixelsPerUnit;

                Vector2 uv = localPos / rend.sprite.bounds.size;
                Texture2D tex = rend.sprite.texture as Texture2D;

                int x = (int)(uv.x * tex.width);
                int y = (int)(uv.y * tex.height);
                var pix = tex.GetPixel(x, y);

                if(pix.a > 0.1f)
                {
                    SurfaceMaterial sf = go.GetComponent<SurfaceMaterial>();
                    if(sf == null)
                    {
                        continue;
                    }

                    return sf;
                }
            }

            return null;
        }

#if UNITY_EDITOR
        private const float GIZMO_SCALE = 0.001f;
        private void OnDrawGizmos()
        {
            if(_rigidbody == null || _carData == null)
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

            Gizmos.DrawRay(_gripRay);
        }
#endif
    }
}