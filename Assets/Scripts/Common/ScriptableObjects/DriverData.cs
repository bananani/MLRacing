using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/DriverData")]
    public class DriverData : ScriptableObject
    {
        [Header("Driver info")]
        public string Name;
        public CarCustomizationData PersonalCarCustomizationSettings;
    }
}