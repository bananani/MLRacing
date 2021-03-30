namespace Common.DataModels.Debug
{
    public struct DriverDebugData
    {
        public string DriverType { get; }

        public DriverDebugData(string driverType) : this()
        {
            DriverType = driverType;
        }
    }
}