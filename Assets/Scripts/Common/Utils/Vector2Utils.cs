using System.Collections;
using UnityEngine;

namespace Common.Utils
{
    public static class Vector2Utils
    {
        public static Vector2 GetRotatedVelocityVector(Vector2 original, float degOfRotation)
        {
            Vector2 velocity = original;
            float rotation = Mathf.Deg2Rad * degOfRotation;
            float cos = Mathf.Cos(rotation);
            float sin = Mathf.Sin(rotation);

            float x = (velocity.x * cos) - (velocity.y * sin);
            float y = (velocity.x * sin) + (velocity.y * cos);

            return new Vector2(x, y);
        }
    }
}