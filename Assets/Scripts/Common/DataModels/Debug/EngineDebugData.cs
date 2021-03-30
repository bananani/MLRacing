namespace Common.DataModels.Debug
{
    public struct EngineDebugData
    {
        public float GeneratedTorque { get; }

        public EngineDebugData(float generatedTorque) : this()
        {
            GeneratedTorque = generatedTorque;
        }
    }
}