using System;
using Common.Components.CarParts;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.DataModels
{
    [Serializable]
    public class RaceEntrant
    {
        public DriverData Driver;
        public Car Car;

        public RaceEntrant(DriverData driverData, Car car)
        {
            Driver = driverData;
            Car = car;
        }
    }
}