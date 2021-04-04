using Common.Components.CarParts;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.DataModels
{
    public readonly struct RaceEntrant
    {
        public DriverData Driver { get; }
        public Car Car { get; }

        public RaceEntrant(DriverData driverData, Car car)
        {
            Driver = driverData;
            Car = car;
        }
    }
}