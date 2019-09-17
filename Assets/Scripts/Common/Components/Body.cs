using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Collider2D))]
    public class Body : MonoBehaviour
    {
        private Collider2D _collider;

        void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        void OnCollisionEnter2D(Collision2D collider)
        {
            Debug.Log("Look where you're going, nutter!");
        }
    }
}