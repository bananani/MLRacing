using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(SpriteRenderer))]
    public class Body : MonoBehaviour
    {
        private CarCustomizationData _customizationData;

        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;
        private Material _carMaterial;

        void Awake()
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

        void OnCollisionEnter2D(Collision2D collider)
        {
            Debug.Log("Look where you're going, nutter!");
        }
    }
}