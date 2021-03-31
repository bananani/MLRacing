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

        private float BodyLength => _collider.bounds.size.y + (_collider.edgeRadius * 2);
        private float BodyWidth => _collider.bounds.size.x + (_collider.edgeRadius * 2);
        private float BodyWidest => Mathf.Sqrt((BodyLength * BodyLength) + (BodyWidth * BodyWidth));
        private float AngleAtWidest => Mathf.Asin((BodyLength * Mathf.Sin(90)) / BodyWidest);

        public float CalculateBodySurfaceArea(float driftAngle, float height) =>
            (driftAngle < AngleAtWidest ?
                Mathf.Lerp(BodyWidth, BodyWidest, driftAngle / AngleAtWidest) :
                Mathf.Lerp(BodyWidest, BodyLength, (driftAngle - AngleAtWidest) / (90 - AngleAtWidest))) * height;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _carMaterial = _renderer.material;
        }

        public void Init(CarCustomizationData customizationData)
        {
            _customizationData = customizationData;
            SetCustomization();
        }

        public void SetCustomization()
        {
            _carMaterial.SetColor("_CustomizationColorR", _customizationData.BaseColor);
            _carMaterial.SetColor("_CustomizationColorG", _customizationData.Customization1);
            _carMaterial.SetColor("_CustomizationColorB", _customizationData.Customization2);
        }

        private void OnCollisionEnter2D(Collision2D collider)
        {
            Debug.Log("Look where you're going, nutter!");
        }
    }
}