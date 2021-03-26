using Common.Constants;
using Common.Identifiers;
using Common.ScriptableObjects;
using Common.Utils;
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
        private ParticleSystem.EmissionModule _emissionModule;

        private float _currentBrakingForce;
        private float _rawInputForce;

        // TODO: Figure out a way to do shifting center of mass and calculate grip using that
        private float _groundContactMultiplier = 1f;
        // How much weight does this tyre have to carry (Vehicle total mass / amount of contact points)
        private float _massResponsibility => _rearAxel ? _carData.RearAxelMassResponsibility * 0.5f : _carData.FrontAxelMassResponsibility * 0.5f;

        // How much weight is put onto this tyre on this frame
        private float _downwardsForce => _massResponsibility * _groundContactMultiplier;
        private float _effectiveGrip => _tyreData.CompoundBaseFriction * _massResponsibility * _groundContactMultiplier;
        private float _currentForceMomentumResponsibility => _massResponsibility * (_currentVelocityInMetersPerSecond / Time.fixedDeltaTime);
        // Getter for the wing effect
        private float _downforceEffect => (_rearAxel ? _carData.BodyKit?.RearWingDownforce : _carData.BodyKit?.FrontSplitterDownforce) ?? 0f;

        // Determines where this tyre is located in the car
        private TyrePositionIdentifier _tyrePosition;
        // Determines if the the tyre is on the rear axel. Returns false if on front axel
        private bool _rearAxel => _tyrePosition == TyrePositionIdentifier.RL || _tyrePosition == TyrePositionIdentifier.RR;
        // Getter for the tyre data of this tyre
        private TyreData _tyreData => _rearAxel ? _carData.RearTyres : _carData.FrontTyres;

        // Determines the current surface this tyre is on
        private SurfaceMaterial _currentSurface;
        // Determines the current forwards grip coefficient relative to the surface material
        private float _surfaceForwardsGripCoefficient => _currentSurface?.ForwardsGripCoefficient ?? 1f;
        // Determines the current sideways grip coefficient relative to the surface material
        private float _surfaceSidewaysGripCoefficient => _currentSurface?.SidewaysGripCoefficient ?? 1f;
        // Determines the current forwards grip coefficient relative to the forwards velocity of the tyre
        private float _forwardsGripCoefficient => _tyreData.GetFrictionCoefficientAtSpeed(Mathf.Abs(_currentForwardsVelocityInMetersPerSecond) * _tyreData.ForwardsFrictionCoefficient);
        // Determines the current sideways grip coefficient relative to the sideways velocity of the tyre
        private float _sidewaysGripCoefficient => _tyreData.GetFrictionCoefficientAtSpeed(Mathf.Abs(_currentSidewaysVelocityInMetersPerSecond) * _tyreData.SidewaysFrictionCoefficient);
        // Determines the current grip coefficient relative to the forwards velocity of the tyre when brakes are locked
        private float _lockedBrakesGripCoefficient => _tyreData.GetFrictionCoefficientAtSpeed(Mathf.Abs(_currentForwardsVelocityInMetersPerSecond) * _tyreData.SidewaysFrictionCoefficient);

        private Vector2 _relativeVelocity => Vector2Utils.GetRotatedVelocityVector(_rigidbody.velocity, -transform.eulerAngles.z);
        private float _currentVelocityInMetersPerSecond => _rigidbody.velocity.magnitude;

        // Drift variables
        private float _tyreAngleRelativeToForwardsVelocity => Vector2.Angle(_rigidbody.velocity, Vector2Utils.GetRotatedVelocityVector(_forwards, transform.rotation.eulerAngles.z));
        private float _tyreAngleRelativeToBackwardsVelocity => Vector2.Angle(_rigidbody.velocity, Vector2Utils.GetRotatedVelocityVector(-_forwards, transform.rotation.eulerAngles.z));

        private float _currentTorque;
        private Vector2 _currentForwardsVelocity => new Vector2(0, _relativeVelocity.y);
        private Vector2 _currentSidewaysVelocity => new Vector2(_relativeVelocity.x, 0);

        private float _currentForwardsVelocityInMetersPerSecond => Mathf.Abs(_currentForwardsVelocity.magnitude);
        private float _currentSidewaysVelocityInMetersPerSecond => Mathf.Abs(_currentSidewaysVelocity.magnitude);

        private Vector2 _currentFramePlayerInitiatedForces;
        private Vector2 _currentFramePhysicsForces;

        // Determines if the car is under acceleration caused by the player input
        private bool _accelerating => _rawInputForce != 0;

        private Ray _gripRay => new Ray(transform.position, Vector3.forward * 1000f);

        private void Start()
        {
            _originalPosition = transform.localPosition;
            _trailRenderer = GetComponent<TrailRenderer>();
            _smokeSystem = GetComponent<ParticleSystem>();
            _emissionModule = _smokeSystem.emission;
        }

        public void Init(CarData carData, TyrePositionIdentifier tyrePosition)
        {
            _carData = carData;
            _tyrePosition = tyrePosition;
            UpdateTyreMass();
        }

        private void Update()
        {
            HoldOriginalPosition();
            _currentSurface = GetSurface();
        }

        private void FixedUpdate()
        {
            Reset();

            CalculateDownforce();
            CalculatePlayerInitiatedForces();
            CalculatePhysics();
            ApplyPhysics();

            ApplyEffects();
        }

        private void Reset()
        {
            _currentFramePhysicsForces = Vector2.zero;
            _currentFramePlayerInitiatedForces = Vector2.zero;
        }

        private void HoldOriginalPosition() => transform.localPosition = _originalPosition;

        private void CalculateDownforce()
        {
            if(_downforceEffect == 0f)
            {
                _groundContactMultiplier = 1f;
                return;
            }

            // Downforce effect = <set downforce coefficient> * <forwards velocity>^2
            float downforceEffect = _downforceEffect * (_currentForwardsVelocityInMetersPerSecond * _currentForwardsVelocityInMetersPerSecond);
            // Full downforce effect has to be divided among both tyres
            downforceEffect *= 0.5f;
            // Calculate downforce effect relative to fixedDeltaTime
            downforceEffect *= CAero.AERO_EFFECTIVENESS_MULTIPLIER;

            _groundContactMultiplier = 1f + downforceEffect;
        }

        private void CalculatePlayerInitiatedForces()
        {
            _currentTorque = (_rawInputForce * (1f - _currentBrakingForce)) / _tyreData.Radius;
            _currentFramePlayerInitiatedForces = _forwards * _currentTorque;
        }

        private void CalculatePhysics()
        {
            _currentFramePhysicsForces += GetSidewaysFriction();
            _currentFramePhysicsForces += GetRollingFriction();
            _currentFramePhysicsForces += GetBrakingFriction();
        }

        private void ApplyPhysics() => _rigidbody.AddRelativeForce(_currentFramePlayerInitiatedForces + _currentFramePhysicsForces);

        private void ApplyEffects()
        {
            bool drifting = _sidewaysGripCoefficient < CTyre.TYRE_TRAIL_SLIP_THRESHOLD;
            bool lockedBrakes = !IsTyreRolling;

            _trailRenderer.emitting = drifting || lockedBrakes;
            _emissionModule.enabled = drifting || lockedBrakes;
        }

        public void UpdateTyreMass() => _rigidbody.mass = _tyreData.Mass;
        public void Accelerate(float force) => _rawInputForce = force;
        public void Brake(float strength) => _currentBrakingForce = strength;
        public void Turn(float degrees) => transform.localEulerAngles = new Vector3(0, 0, GetToeAngle() + degrees);

        private Vector2 GetSidewaysFriction() => -_currentSidewaysVelocity * _surfaceSidewaysGripCoefficient * _sidewaysGripCoefficient * _effectiveGrip;
        private Vector2 GetRollingFriction() => -_currentForwardsVelocity * CTyre.ROLLING_RESISTANCE * CTyre.ROLLING_RESISTANCE * _effectiveGrip;
        private Vector2 GetBrakingFriction() => -_currentForwardsVelocity * _currentBrakingForce * _effectiveGrip * (IsTyreRolling ? _surfaceForwardsGripCoefficient * _forwardsGripCoefficient : _surfaceSidewaysGripCoefficient * _lockedBrakesGripCoefficient);

        private bool IsTyreRolling => _currentBrakingForce < CTyre.BRAKE_LOCK_THRESHOLD;

        private float GetToeAngle()
        {
            switch(_tyrePosition)
            {
                case TyrePositionIdentifier.FL:
                    return -_carData.ToeFront;
                case TyrePositionIdentifier.FR:
                    return _carData.ToeFront;
                case TyrePositionIdentifier.RL:
                    return -_carData.ToeRear;
                case TyrePositionIdentifier.RR:
                    return _carData.ToeRear;
                default:
                    return 0;
            }
        }

        public float GetDriftAngle() => Mathf.Min(_tyreAngleRelativeToForwardsVelocity, _tyreAngleRelativeToBackwardsVelocity);

        private SurfaceMaterial GetSurface()
        {
            return null;
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
        private const float GIZMO_SCALE = 0.005f;
        private void OnDrawGizmos()
        {
            if(_rigidbody == null || _carData == null)
            {
                return;
            }

            Vector2 locationIn2DSpace = new Vector2(transform.position.x, transform.position.y);

            DrawRollingFrictionGizmo(locationIn2DSpace, Color.yellow);
            DrawSidewaysFrictionGizmo(locationIn2DSpace, Color.red, Color.green);
            DrawBrakingFrictionGizmo(locationIn2DSpace, Color.red, Color.green);

            Gizmos.DrawRay(_gripRay);
        }

        private void DrawRollingFrictionGizmo(Vector2 locationIn2DSpace, Color gizmoColor)
        {
            Vector2 correctedRollingFrictionForce = Vector2Utils.GetRotatedVelocityVector(GetRollingFriction(), transform.rotation.eulerAngles.z) * GIZMO_SCALE;

            Gizmos.color = gizmoColor;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedRollingFrictionForce);
        }

        private void DrawSidewaysFrictionGizmo(Vector2 locationIn2DSpace, Color noGripGizmoColor, Color maxGripGizmoColor)
        {
            float minFrictionCoefficient = _tyreData.MinFrictionCoefficient() * _tyreData.SidewaysFrictionCoefficient;
            float maxFrictionCoefficient = _tyreData.MaxFrictionCoefficient() * _tyreData.SidewaysFrictionCoefficient;
            float currentRelativeCoefficient = (_sidewaysGripCoefficient - minFrictionCoefficient) / (maxFrictionCoefficient - minFrictionCoefficient);
            Gizmos.color = Color.Lerp(noGripGizmoColor, maxGripGizmoColor, currentRelativeCoefficient);

            Vector2 correctedSidewaysFrictionForce = Vector2Utils.GetRotatedVelocityVector(GetSidewaysFriction(), transform.rotation.eulerAngles.z) * GIZMO_SCALE;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedSidewaysFrictionForce);
        }

        private void DrawBrakingFrictionGizmo(Vector2 locationIn2DSpace, Color noGripGizmoColor, Color maxGripGizmoColor)
        {
            float minFrictionCoefficient = _tyreData.MinFrictionCoefficient() * (IsTyreRolling ? _tyreData.ForwardsFrictionCoefficient : _tyreData.SidewaysFrictionCoefficient);
            float maxFrictionCoefficient = _tyreData.MaxFrictionCoefficient() * (IsTyreRolling ? _tyreData.ForwardsFrictionCoefficient : _tyreData.SidewaysFrictionCoefficient);
            float currentRelativeCoefficient = ((IsTyreRolling ? _forwardsGripCoefficient : _lockedBrakesGripCoefficient) - minFrictionCoefficient) / (maxFrictionCoefficient - minFrictionCoefficient);
            Gizmos.color = Color.Lerp(noGripGizmoColor, maxGripGizmoColor, currentRelativeCoefficient);

            Vector2 correctedBrakingFrictionForce = Vector2Utils.GetRotatedVelocityVector(GetBrakingFriction(), transform.rotation.eulerAngles.z) * GIZMO_SCALE;
            Gizmos.DrawLine(locationIn2DSpace, locationIn2DSpace + correctedBrakingFrictionForce);
        }
#endif
    }
}