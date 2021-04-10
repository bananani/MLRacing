using System;
using UnityEditor;
using UnityEngine;
using Common.Attributes;

namespace CommonEditor.CustomDrawers
{
    [CustomPropertyDrawer(typeof(ReversibleArrayAttribute), false)]
    public class ReversibleArrayDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool button = GUI.Button(position, "Reverse array elements");
            property.NextVisible(false);

            if(!property.isArray)
            {
                return;
            }

            if(button)
            {
                ReverseArrayEntries(ref property);
            }
        }

        public static void ReverseArrayEntries(ref SerializedProperty property)
        {
            if(!property.isArray)
            {
                return;
            }

            int originalArraySize = property.arraySize;

            for(int i = 0; i < originalArraySize; i++)
            {
                property.MoveArrayElement(i, 0);
            }
        }
    }
}