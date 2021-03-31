using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BodyKitData")]
    public class BodyKitData : ScriptableObject
    {
        [Header("Air resistance")]
        [Min(0)]
        public float CarHeight;
        [Min(0)]
        public float AirResistanceCoefficient;

        [Header("Aero")]
        [Min(0)]
        public float FrontWingWidth;
        [Min(0), Tooltip("Thickness of the wing from the front")]
        public float FrontWingChord;
        [Range(0f, 2f)]
        public float FrontSplitterLiftCoefficient;

        [Min(0)]
        public float RearWingWidth;
        [Min(0), Tooltip("Thickness of the wing from the front")]
        public float RearWingChord;
        [Range(0f, 2f)]
        public float RearWingLiftCoefficient;

        public float FrontSplitterSurfaceArea => FrontWingChord * FrontWingWidth;
        public float RearWingSurfaceArea => RearWingChord * RearWingWidth;
    }
}