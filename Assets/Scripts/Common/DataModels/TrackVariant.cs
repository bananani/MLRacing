using System;
using Common.Components.TrackParts;
using UnityEngine;

namespace Common.DataModels
{
    [Serializable]
    public struct TrackVariant
    {
        public int Id;
        public Transform[] ObstacleGroups;
        public Transform[] StartingGrid;
        public Checkpoint[] Checkpoints;

        public TrackVariant(int id, Transform[] obstacleGroups, Transform[] startingGrid, Checkpoint[] checkpoints)
        {
            Id = id;
            ObstacleGroups = obstacleGroups;
            StartingGrid = startingGrid;
            Checkpoints = checkpoints;
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