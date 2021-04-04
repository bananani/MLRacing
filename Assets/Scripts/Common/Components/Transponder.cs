using Common.Interfaces;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class Transponder : MonoBehaviour
    {
        public delegate void TransponderEvent();
        public event TransponderEvent CheckpointReached;

        [SerializeField]
        private Collider2D _collider;

        private int _checkpointLayer = 0;

        public float CurrentTime => Time.time;

        public void OnValidate()
        {
            if(_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }

        public void Awake() => Init();
        public void Init()
        {
            _checkpointLayer = LayerMask.NameToLayer("Checkpoint");
            if(_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if(other.gameObject.layer == _checkpointLayer && other.GetComponent<ICheckpointSensor>() is ICheckpointSensor sensor)
            {
                sensor.Report(this);
            }
        }
    }
}