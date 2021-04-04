using System.Collections.Generic;
using Common.Components.TrackParts;
using Common.DataModels;
using UnityEngine;

namespace Common.Managers
{
    public class TrackManager : MonoBehaviour
    {
        public delegate void TrackVariantEvent(TrackVariant variant);
        public event TrackVariantEvent InitializationCompleted;

        [SerializeField]
        private int _selectedTrackVariant;
        [SerializeField]
        private TrackVariant[] _trackVariants = new TrackVariant[0];

        private readonly Dictionary<int, TrackVariant> _trackVariantLookup = new Dictionary<int, TrackVariant>();
        private readonly Dictionary<int, Checkpoint> _checkpointLookup = new Dictionary<int, Checkpoint>();

        private int _currentTrackVariant = -1;

        public TrackVariant CurrentTrackVariant => _trackVariantLookup[_currentTrackVariant];
        public int CheckpointCount => _checkpointLookup.Count;

        public void Awake() => InitializeTrackVariant(_selectedTrackVariant);

        public void OnValidate()
        {
            if(_trackVariants.Length == 0)
            {
                return;
            }

            int firstVariant = int.MinValue;
            foreach(TrackVariant variant in _trackVariants)
            {
                if(variant.Id == _selectedTrackVariant)
                {
                    return;
                }

                if(firstVariant == int.MinValue)
                {
                    firstVariant = variant.Id;
                }
            }

            // SelectedVariant is not setup in _trackVariants, selecting first available variant
            _selectedTrackVariant = firstVariant;
        }

        public void Update()
        {
            if(_selectedTrackVariant != _currentTrackVariant)
            {
                InitializeTrackVariant(_selectedTrackVariant);
            }
        }

        public void InitializeTrackVariant(int variantId)
        {
            if(_currentTrackVariant == variantId)
            {
                // Already initialized
                return;
            }

            IndexTrackVariantInformation();
            if(!_trackVariantLookup.TryGetValue(variantId, out TrackVariant variant))
            {
                // Variant not valid
                return;
            }

            DisableAllVariantObstacles();
            DisableAllVariantCheckpoints();
            SetVariantObstaclesEnabled(variant, true);
            SetVariantCheckpointsEnabled(variant, true);

            _checkpointLookup.Clear();

            for(int i = 0; i < variant.Checkpoints.Length; i++)
            {
                variant.Checkpoints[i].Init(i, i == variant.Checkpoints.Length - 1);
                _checkpointLookup.Add(i, variant.Checkpoints[i]);
            }

            _currentTrackVariant = variantId;
            InitializationCompleted?.Invoke(variant);
        }

        public bool GetNextCheckpoint(Checkpoint checkpoint, out Checkpoint nextCheckpoint)
        {
            if(_checkpointLookup.TryGetValue(checkpoint.CheckpointIndex + 1, out nextCheckpoint))
            {
                // Still on the same lap
                return false;
            }

            if(_checkpointLookup.TryGetValue(0, out nextCheckpoint))
            {
                // Next lap started
                return true;
            }

            return false;
        }

        public void SetVariantObstaclesEnabled(TrackVariant variant, bool enable)
        {
            foreach(Transform t in variant.ObstacleGroups)
            {
                if(t?.gameObject == null)
                {
                    return;
                }

                t.gameObject.SetActive(enable);
            }
        }

        public void SetVariantCheckpointsEnabled(TrackVariant variant, bool enable)
        {
            foreach(Checkpoint t in variant.Checkpoints)
            {
                if(t?.gameObject == null)
                {
                    return;
                }

                t.gameObject.SetActive(enable);
            }
        }

        public void DisableAllVariantObstacles()
        {
            foreach(TrackVariant variant in _trackVariantLookup.Values)
            {
                SetVariantObstaclesEnabled(variant, false);
            }
        }

        public void DisableAllVariantCheckpoints()
        {
            foreach(TrackVariant variant in _trackVariantLookup.Values)
            {
                SetVariantCheckpointsEnabled(variant, false);
            }
        }

        private void IndexTrackVariantInformation()
        {
            _trackVariantLookup.Clear();
            foreach(TrackVariant variant in _trackVariants)
            {
                if(_trackVariantLookup.ContainsKey(variant.Id))
                {
                    UnityEngine.Debug.LogWarning($"TrackVariant ID conflict: {variant.Id}. Some variants are unavailable");
                    continue;
                }

                _trackVariantLookup[variant.Id] = variant;
            }
        }
    }
}