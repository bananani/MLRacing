using Common.Constants;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.CarParts
{
    [RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(SpriteRenderer))]
    public class Body : MonoBehaviour
    {
        private CarCustomizationData _customizationData;

        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;
        private Material _carMaterial;

        public float BodyLength => _collider.bounds.size.y + (_collider.edgeRadius * 2);
        public float BodyWidth => _collider.bounds.size.x + (_collider.edgeRadius * 2);
        public float BodyWidest => Mathf.Sqrt((BodyLength * BodyLength) + (BodyWidth * BodyWidth));
        public float AngleAtWidest => Mathf.Asin((BodyLength * Mathf.Sin(90)) / BodyWidest);

        [Header("Air resistance")]
        [SerializeField, Min(0)]
        private float CarHeight;
        [SerializeField, Min(0), Tooltip("How aerodynamic is the car? (Racing car: ~0.2, Cube: ~1.05)")]
        private float AirResistanceCoefficient;

        public float CalculateBodySurfaceArea(float driftAngle, float height) =>
            (driftAngle < AngleAtWidest ?
                Mathf.Lerp(BodyWidth, BodyWidest, driftAngle / AngleAtWidest) :
                Mathf.Lerp(BodyWidest, BodyLength, (driftAngle - AngleAtWidest) / (90 - AngleAtWidest))) * height;

        public float GetAirResistance(float airVelocity, float driftAngle) => 0.5f * CAero.AIR_DENSITY * (airVelocity * airVelocity) * AirResistanceCoefficient * CalculateBodySurfaceArea(driftAngle, CarHeight);

        public void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _carMaterial = _renderer.material;
        }

        public void Init(CarCustomizationData customizationData) => SetCustomization(customizationData);

        public void SetCustomization() => SetCustomization(_customizationData);

        public void SetCustomization(CarCustomizationData customizationData)
        {
            _customizationData = customizationData;

            _carMaterial.SetColor("_CustomizationColorR", _customizationData.BaseColor);
            _carMaterial.SetColor("_CustomizationColorG", _customizationData.Customization1);
            _carMaterial.SetColor("_CustomizationColorB", _customizationData.Customization2);
        }

        public void OnCollisionEnter2D(Collision2D collider)
        {
            UnityEngine.Debug.Log("Look where you're going, nutter!");
        }
    }
}