using Common.Components.CarParts;
using Common.DataModels.Debug;
using Common.ScriptableObjects;
using UnityEngine;

namespace Common.Components.Drivers
{
    [RequireComponent(typeof(Car))]
    public abstract class Driver : MonoBehaviour
    {
        [SerializeField]
        private DriverData _driverData;
        [SerializeField]
        private Car _car;

        public void OnValidate() => GetComponentReferences();
        public void Start() => GetComponentReferences();
        public void Update() => DriverUpdate();
        public void FixedUpdate() => DriverFixedUpdate();

        private void GetComponentReferences() => _car ??= GetComponent<Car>();

        public void SetupDriver() => SetupDriver(_driverData);
        public void SetupDriver(DriverData driverData) => _driverData = driverData;

        protected abstract void DriverUpdate();
        protected abstract void DriverFixedUpdate();

        protected void Accelerate(float strength) => _car.Accelerate(strength);
        protected void Brake(float strength) => _car.Brake(strength);
        protected void Handbrake(float strength) => _car.Handbrake(strength);
        protected void Turn(float strength) => _car.Turn(strength);

        public DebugData CollectDebugData() => new DebugData(new DriverDebugData(Time.deltaTime.ToString()), _car.CollectDebugData());
    }
}