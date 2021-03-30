using UnityEngine;

namespace Common.Components
{
    public class SurfaceMaterial : MonoBehaviour
    {
        public string SurfaceName;
        public float SidewaysGripCoefficient;
        public float ForwardsGripCoefficient;

        public bool TyreTrailsAlways;
        public bool TyreSmokeAlways;

        public int Priority;
    }
}