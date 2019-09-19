using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(BoxCollider2D)), RequireComponent(typeof(SpriteRenderer))]
    public class Body : MonoBehaviour
    {
        private CarData _carData;

        private BoxCollider2D _collider;
        private SpriteRenderer _renderer;
        private Material _carMaterial;

        void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _renderer = GetComponent<SpriteRenderer>();
            _carMaterial = _renderer.material;
        }

        public void Init(CarData carData)
        {
            _carData = carData;
            SetCustomization();
        }

        public void SetCustomization()
        {
            _carMaterial.SetColor("_CustomizationColorR", _carData.BaseColor);
            _carMaterial.SetColor("_CustomizationColorG", _carData.Customization1);
            _carMaterial.SetColor("_CustomizationColorB", _carData.Customization2);
        }

        void OnCollisionEnter2D(Collision2D collider)
        {
            Debug.Log("Look where you're going, nutter!");
        }
    }
}