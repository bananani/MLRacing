using Common.Identifiers;
using Common.Interfaces;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.TrackParts
{
    public class CheckpointOnTrack : MonoBehaviour, ICheckpointSensor
    {
        private Checkpoint _checkpoint;

        public void Init(Checkpoint checkpoint) => _checkpoint = checkpoint;
        public void Report(Transponder transponder) => _checkpoint?.DriverReportedOnTrack(transponder);

#if UNITY_EDITOR
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.black;

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