using UnityEngine;

namespace Common.Components.Drivers
{
    public class DebugPlayer : Driver
    {
        private float _acceleration;
        private float _brake;
        private float _handbrake;
        private float _turn = 0f;

        [SerializeField]
        private bool _smoothTurning = true;
        [SerializeField]
        private float _turnPerSecond = 4f;
        [SerializeField]
        private float _returnMultiplier = 4f;

        protected override void DriverUpdate()
        {
            _acceleration = EvaluateAcceleration();
            _brake = EvaluateBrake();
            _handbrake = EvaluateHandbrake();
            _turn = EvaluateTurning();
        }

        protected override void DriverFixedUpdate()
        {
            Accelerate(_acceleration);
            Brake(_brake);
            Handbrake(_handbrake);
            Turn(_turn);
        }

        private float EvaluateAcceleration()
        {
            if(Input.GetKey(KeyCode.W))
            {
                if(Input.GetKey(KeyCode.S))
                {
                    return -1f;
                }
                else
                {
                    return 1f;
                }
            }

            return 0f;
        }

        private float EvaluateBrake()
        {
            if(Input.GetKey(KeyCode.W))
            {
                return 0f;
            }
            else if(Input.GetKey(KeyCode.S))
            {
                return 1f;
            }

            return 0f;
        }

        private float EvaluateHandbrake()
        {
            if(Input.GetKey(KeyCode.Space))
            {
                return 1f;
            }

            return 0f;
        }

        private float EvaluateTurning()
        {
            float turn = _turn;
            if(!_smoothTurning)
            {
                turn = 0f;
                if(Input.GetKey(KeyCode.A))
                {
                    turn += 1f;
                }

                if(Input.GetKey(KeyCode.D))
                {
                    turn -= 1f;
                }

                return turn;
            }

            if(!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                if(turn < 0)
                {
                    turn += _turnPerSecond * Time.deltaTime * _returnMultiplier;
                    if(turn > 0)
                    {
                        turn = 0;
                    }
                }
                else if(turn > 0)
                {
                    turn -= _turnPerSecond * Time.deltaTime * _returnMultiplier;
                    if(turn < 0)
                    {
                        turn = 0;
                    }
                }
            }

            if(Input.GetKey(KeyCode.A))
            {
                turn += _turnPerSecond * Time.deltaTime;

                if(turn < 0)
                {
                    turn += _turnPerSecond * Time.deltaTime * _returnMultiplier;
                }
            }

            if(Input.GetKey(KeyCode.D))
            {
                turn -= _turnPerSecond * Time.deltaTime;

                if(turn > 0)
                {
                    turn -= _turnPerSecond * Time.deltaTime * _returnMultiplier;
                }
            }

            return Mathf.Clamp(turn, -1f, 1f);
        }
    }
}