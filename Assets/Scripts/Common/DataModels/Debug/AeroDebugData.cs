namespace Common.DataModels.Debug
{
    public struct AeroDebugData
    {
        public float FrontSplitterDownforce { get; }
        public float RearWingDownforce { get; }
        public float CurrentAirResistance { get; }

        public AeroDebugData(float frontSplitterDownforce, float rearWingDownforce, float currentAirResistance) : this()
        {
            FrontSplitterDownforce = frontSplitterDownforce;
            RearWingDownforce = rearWingDownforce;
            CurrentAirResistance = currentAirResistance;
        }
    }
}