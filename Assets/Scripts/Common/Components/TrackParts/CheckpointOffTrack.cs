using Common.Identifiers;
using Common.Interfaces;
using UnityEngine;

namespace Common.Components.TrackParts
{
    public class CheckpointOffTrack : MonoBehaviour, ICheckpointSensor
    {
        private static readonly Color _orange = new Color(1f, 0.5f, 0f);
        
        [SerializeField]
        private InfractionSeverityIdentifier _infractionSeverity;

        private Checkpoint _checkpoint;

        public void Init(Checkpoint checkpoint) => _checkpoint = checkpoint;
        public void Report(Transponder transponder) => _checkpoint?.DriverReportedOffTrack(transponder, _infractionSeverity);

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = _infractionSeverity switch
            {
                InfractionSeverityIdentifier.OnTrack => Color.black,
                InfractionSeverityIdentifier.OffTrack => Color.green,
                InfractionSeverityIdentifier.InCornerEnd => Color.yellow,
                InfractionSeverityIdentifier.InCorner => _orange,
                InfractionSeverityIdentifier.InCornerApex => Color.red
            };

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if(collider == null)
            {
                return;
            }

            Matrix4x4 matrix = transform.localToWorldMatrix;
            Gizmos.matrix = matrix;
            Gizmos.DrawWireCube(collider.offset, collider.size);
        }
#endif
    }
}