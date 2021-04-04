using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BodyKitData")]
    public class DriverData : ScriptableObject
    {
        [Header("Driver info")]
        public string Name;
        public CarCustomizationData PersonalCarCustomizationSettings;
    }
}