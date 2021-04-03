using System.Collections.Generic;
using Common.DataModels;
using UnityEngine;

namespace Common.Components
{
    public class TrackVariantSelector : MonoBehaviour
    {
        public delegate void TrackVariantEvent(TrackVariant variant);
        public static event TrackVariantEvent InitializationCompleted;

        [SerializeField]
        private int _selectedTrackVariant;
        [SerializeField]
        private TrackVariant[] _trackVariants = new TrackVariant[0];

        private Dictionary<int, TrackVariant> _trackVariantLookup = new Dictionary<int, TrackVariant>();

        private int _currentTrackVariant = -1;

        public void Awake()
        {
            IndexTrackVariantInformation();
            InitializeTrackVariant(_selectedTrackVariant);
        }

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

            if(!_trackVariantLookup.TryGetValue(variantId, out TrackVariant variant))
            {
                // Variant not valid
                return;
            }

            DisableAllVariantObstacles();
            SetVariantObstaclesEnabled(variant, true);
            _currentTrackVariant = variantId;
            InitializationCompleted?.Invoke(variant);
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

        public void DisableAllVariantObstacles()
        {
            foreach(TrackVariant variant in _trackVariantLookup.Values)
            {
                SetVariantObstaclesEnabled(variant, false);
            }
        }

        private void IndexTrackVariantInformation()
        {
            _trackVariantLookup.Clear();
            foreach(TrackVariant variant in _trackVariants)
            {
                if(_trackVariantLookup.ContainsKey(variant.Id))
                {
                    Debug.LogWarning($"TrackVariant ID conflict: {variant.Id}. Some variants are unavailable");
                    continue;
                }

                _trackVariantLookup[variant.Id] = variant;
            }
        }
    }
}