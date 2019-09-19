using UnityEngine;

namespace Common.Components
{
    [RequireComponent(typeof(Car))]
    public abstract class Driver : MonoBehaviour
    {
        private Car _car;

        private void Start()
        {
            _car = GetComponent<Car>();
        }

        private void FixedUpdate()
        {
            UpdateDriver();
        }

        protected abstract void UpdateDriver();

        protected void Accelerate(float strength)
        {
            _car.Accelerate(strength);
        }

        protected void Brake(float strength)
        {
            _car.Brake(strength);
        }

        protected void Handbrake(float strength)
        {
            _car.Handbrake(strength);
        }

        protected void Turn(float strength)
        {
            _car.Turn(strength);
        }
    }
}