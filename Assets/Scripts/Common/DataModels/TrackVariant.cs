using System;
using Common.Attributes;
using Common.Components.TrackParts;
using UnityEngine;

namespace Common.DataModels
{
    [Serializable]
    public class TrackVariant
    {
        public int Id;
        public string Name;
        public Transform[] ObstacleGroups;
        public Transform[] StartingGrid;
        [ReversibleArray, Tooltip("This button reverses the order of the following serialized array")]
        public EditorReverseArrayButton ReverseButton;
        public Checkpoint[] Checkpoints;


        public TrackVariant(int id, string name, Transform[] obstacleGroups, Transform[] startingGrid, Checkpoint[] checkpoints)
        {
            Id = id;
            Name = name;
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