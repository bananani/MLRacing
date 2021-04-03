using System;
using Common.Identifiers;
using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/CarData")]
    public class CarData : ScriptableObject
    {
        [Header("General info")]
        public string CarName;
        public DrivetrainIdentifier DrivetrainType;
        public SteeringTypeIdentifier SteeringType;

        [Header("Weight")]
        [Min(0)]
        public float VehicleMass;

        [Header("Engine")]
        [Min(0)]
        public float MaxAcceleration;

        [Header("Steering")]
        [Range(0f, 90f)]
        public float MaxSteeringAngle;
        [Range(-30f, 30f)]
        public float ToeFront;
        [Range(-30f, 30f)]
        public float ToeRear;

        [Header("Tyres")]
        public TyreData FrontTyres;
        public TyreData RearTyres;

        [Header("Body Kit")]
        public BodyKitData BodyKit;

        public float CarTotalMass => VehicleMass + (FrontTyres.Mass * 2) + (RearTyres.Mass * 2);
        public float FrontAxelMassResponsibility => (VehicleMass * 0.5f) + (FrontTyres.Mass * 2);
        public float RearAxelMassResponsibility => (VehicleMass * 0.5f) + (RearTyres.Mass * 2);
    }
}