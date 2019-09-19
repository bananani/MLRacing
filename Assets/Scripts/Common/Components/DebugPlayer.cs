using UnityEngine;

namespace Common.Components
{

    public class DebugPlayer : Driver
    {
        protected override void UpdateDriver()
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
                if(Input.GetKey(KeyCode.S))
                {
                    Accelerate(-0.4f);
                    Brake(0);
                }
                else
                {
                    Accelerate(1);
                    Brake(0);
                }
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

            float turn = 0;
            if(Input.GetKey(KeyCode.A))
            {
                turn += 1;
            }

            if(Input.GetKey(KeyCode.D))
            {
                turn -= 1;
            }

            Turn(turn);
        }
    }
}