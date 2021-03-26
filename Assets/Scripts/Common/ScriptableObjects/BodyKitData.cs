using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BodyKitData")]
    public class BodyKitData : ScriptableObject
    {
        [Header("Aero")]
        [Min(0)]
        public float FrontSplitterDownforce;
        [Min(0)]
        public float RearWingDownforce;
    }
}