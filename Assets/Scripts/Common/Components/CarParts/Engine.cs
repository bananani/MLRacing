using Common.DataModels.Debug;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.CarParts
{
    public class Engine : MonoBehaviour
    {
        private const int KWH_CONVERSION = 1000;

        private CarData _carData;
        private Drivetrain _drivetrain;
        private float _maxAcceleration => _carData.MaxAcceleration * KWH_CONVERSION;

        public void Init(Drivetrain drivetrain, CarData carData)
        {
            _drivetrain = drivetrain;
            _carData = carData;
        }

        public void Accelerate(float strength)
        {
            _drivetrain.Accelerate(strength * _maxAcceleration);
        }

        public EngineDebugData CollectDebugData()
        {
            return new EngineDebugData(0f);
        }
    }
}