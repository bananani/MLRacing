using UnityEngine;

namespace Common.Components.Drivers
{
    public class DebugPlayer : Driver
    {
        protected override void UpdateDriver() { }

        protected override void FixedUpdateDriver()
        {
            if(Input.GetKey(KeyCode.Space))
            {
                Handbrake(1);
            }
            else
            {
                Handbrake(0);
            }

            if(Input.GetKey(KeyCode.W))
            {
                Accelerate(1);
                Brake(0);
            }
            else if(Input.GetKey(KeyCode.S))
            {
                Accelerate(0);
                Brake(1);
            }
            else
            {
                Accelerate(0);
                Brake(0);
            }

            float turn = 0f;
            if(Input.GetKey(KeyCode.A))
            {
                turn += 1f;
            }

            if(Input.GetKey(KeyCode.D))
            {
                turn -= 1f;
            }

            Turn(turn);
        }
    }
}