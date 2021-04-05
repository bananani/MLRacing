using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.TrackParts
{
    public delegate void CheckpointEvent(Checkpoint checkpoint, Transponder transponder, OffTrackSeverityIdentifier infractionSeverity);
    public class Checkpoint : MonoBehaviour
    {
        public event CheckpointEvent CheckpointReport;

        [SerializeField]
        private CheckpointOnTrack _trackSensor;
        [SerializeField]
        private CheckpointOffTrack[] _offTrackSensors;

        public int CheckpointIndex { get; private set; }
        public bool IsFinishLine { get; private set; }

        public void OnValidate()
        {
            if(_trackSensor == null)
            {
                _trackSensor = GetComponentInChildren<CheckpointOnTrack>();
            }

            if(_offTrackSensors == null || _offTrackSensors.Length == 0)
            {
                _offTrackSensors = GetComponentsInChildren<CheckpointOffTrack>();
            }
        }

        public void Init(int checkpointIndex, bool isFinishLine)
        {
            CheckpointIndex = checkpointIndex;
            IsFinishLine = isFinishLine;

            _trackSensor.Init(this);
            foreach(CheckpointOffTrack offTrackSensor in _offTrackSensors)
            {
                offTrackSensor.Init(this);
            }
        }

        public void DriverReportedOnTrack(Transponder transponder) => CheckpointReport?.Invoke(this, transponder, OffTrackSeverityIdentifier.OnTrack);

        public void DriverReportedOffTrack(Transponder transponder, OffTrackSeverityIdentifier infractionSeverity) => CheckpointReport?.Invoke(this, transponder, infractionSeverity);
    }
}