using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/CarCustomizationData")]
    public class CarCustomizationData : ScriptableObject
    {
        [Header("Colours")]
        public Color BaseColor = Color.white;
        public Color Customization1 = Color.white;
        public Color Customization2 = Color.white;
    }
}