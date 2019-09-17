using UnityEngine;

namespace Common.Components
{
    public class DebugPlayer : MonoBehaviour
    {
        private Car _car;

        private void Start()
        {
            _car = GetComponent<Car>();
        }

        private void FixedUpdate()
        {
            if(Input.GetKey(KeyCode.Space))
            {
                _car.Handbrake(1);
            }
            else
            {
                _car.Handbrake(0);
            }

            if(Input.GetKey(KeyCode.W))
            {
                if(Input.GetKey(KeyCode.S))
                {
                    _car.Accelerate(-1);
                    _car.Brake(0);
                }
                else
                {
                    _car.Accelerate(1);
                    _car.Brake(0);
                }
            }
            else if(Input.GetKey(KeyCode.S))
            {
                _car.Accelerate(0);
                _car.Brake(1);
            }
            else
            {
                _car.Accelerate(0);
                _car.Brake(0);
            }

            float turn = 0;
            if(Input.GetKey(KeyCode.A))
            {
                turn += 1;
            }

            if(Input.GetKey(KeyCode.D))
            {
                turn -= 1;
            }

            _car.Turn(turn);
        }
    }
}