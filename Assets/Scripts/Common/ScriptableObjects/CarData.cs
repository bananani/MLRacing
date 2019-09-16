using Common.Attributes;
using Common.Identifiers;
using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName="ScriptableObjects/CarData")]
    public class CarData : ScriptableObject
    {
        [Header("General info")]
        public string CarName;
        [EnumFlag]
        public DrivetrainIdentifier DrivetrainType;
        [EnumFlag]
        public SteeringTypeIdentifier SteeringType;

        [Header("Engine")]
        [Min(0)]
        public float MaxAcceleration;

        [Header("Tyres")]
        [Min(float.Epsilon), Tooltip("Tyre radius in mm")]
        public float TyreRadius;
        [Range(0f, 90f)]
        public float MaxSteeringAngle;
        [Min(0)]
        public float Grip;
        [Range(0f, 1f), Tooltip("Grip at locked brakes or at 90 degree drift (Higher = More grip when sliding)")]
        public float MinGripDuringSlip;
        [Range(0f, 90f), Tooltip("Drift angle where tyres start losing grip (Higher = More grip in corners)")]
        public float MinGripLossAngle = 20f;
        [Range(-30f, 30f)]
        public float ToeFront;
        [Range(-30f, 30f)]
        public float ToeRear;
    }
}