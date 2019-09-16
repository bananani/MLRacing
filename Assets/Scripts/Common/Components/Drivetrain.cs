using Common.Identifiers;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components
{
    public class Drivetrain : MonoBehaviour
    {
        private CarData _carData;
        private Axle _frontAxle;
        private Axle _rearAxle;
        private DrivetrainIdentifier _drivetrainType => _carData.DrivetrainType;

        public void Init(Axle frontAxle, Axle rearAxle, CarData carData)
        {
            _frontAxle = frontAxle;
            _rearAxle = rearAxle;
            _carData = carData;
        }

        public void Accelerate(float torque)
        {
            float fwdTorque = (_drivetrainType & DrivetrainIdentifier.FWD) == DrivetrainIdentifier.FWD ? torque : 0f;
            _frontAxle.Accelerate(fwdTorque);

            float rwdTorque = (_drivetrainType & DrivetrainIdentifier.RWD) == DrivetrainIdentifier.RWD ? torque : 0f;
            _rearAxle.Accelerate(rwdTorque);  
        }
    }
}