using System;
using Common.Components.CarParts;
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

        private Car _car;

        private int _checkpointLayer = 0;

        public float CurrentTime => Time.time;

        private Guid _transponderId;
        public Guid TransponderId => _transponderId;

        public void OnValidate()
        {
            if(_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }
        }

        public void Init(Car car)
        {
            _checkpointLayer = LayerMask.NameToLayer("Checkpoint");
            if(_collider == null)
            {
                _collider = GetComponent<Collider2D>();
            }

            _car = car;
            _transponderId = Guid.NewGuid();
        }

        public float GetCarSpeed() => _car.RelativeVelocity.magnitude;
        public float GetCarAcceleration() => _car.Acceleration.magnitude;

        public void OnTriggerEnter2D(Collider2D other)
        {
            if(other.gameObject.layer == _checkpointLayer && other.GetComponent<ICheckpointSensor>() is ICheckpointSensor sensor)
            {
                sensor.Report(this);
            }
        }
    }
}