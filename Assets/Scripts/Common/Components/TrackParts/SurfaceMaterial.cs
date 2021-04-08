using UnityEngine;

namespace Common.Components
{
    public class SurfaceMaterial : MonoBehaviour
    {
        public string SurfaceName;
        [Tooltip("Allowed surfaces do not invalidate laptimes")]
        public bool AllowedSurface;

        public float SidewaysGripCoefficient = 1f;
        public float ForwardsGripCoefficient = 1f;

        public float SurfaceMovementResistanceCoefficient = 0f;

        public bool TyreTrailsAlways;
        public bool TyreSmokeAlways;

        public int Priority;
    }
}