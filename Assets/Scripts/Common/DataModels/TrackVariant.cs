using System;
using UnityEngine;

namespace Common.DataModels
{
    [Serializable]
    public struct TrackVariant
    {
        public int Id;
        public Transform[] ObstacleGroups;
        public Transform[] StartingGrid;
        // Checkpoints

        public TrackVariant(int id, Transform[] obstacleGroups, Transform[] startingGrid)
        {
            Id = id;
            ObstacleGroups = obstacleGroups;
            StartingGrid = startingGrid;
        }
    }

    public static class TrackVariantExtensions
    {
        public static Transform GetGridPositionTransform(this TrackVariant variant, int pos)
        {
            if(variant.StartingGrid == null)
            {
                // Invalid Starting grid
                return null;
            }
            if(pos < 1 || pos > variant.StartingGrid.Length)
            {
                // Invalid grid position
                return null;
            }

            return variant.StartingGrid[pos - 1];
        }
    }
}