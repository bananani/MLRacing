using Common.Identifiers;
using Common.Interfaces;
using UnityEngine;

namespace Common.Components.TrackParts
{
    public class CheckpointOffTrack : MonoBehaviour, ICheckpointSensor
    {
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
                InfractionSeverityIdentifier.OnTrack => Color.green,
                InfractionSeverityIdentifier.Minor => Color.yellow,
                InfractionSeverityIdentifier.Moderate => new Color(1f, 0.5f, 0f),
                InfractionSeverityIdentifier.Severe => Color.red
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