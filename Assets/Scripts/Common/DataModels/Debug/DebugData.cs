namespace Common.DataModels.Debug
{
    public struct DebugData
    {
        // Driver
        public DriverDebugData Driver { get; }

        // Car
        public CarDebugData Car { get; }

        public DebugData(DriverDebugData driver, CarDebugData car) : this()
        {
            Driver = driver;
            Car = car;
        }
    }
}