namespace Common.DataModels.Debug
{
    public struct AeroDebugData
    {
        public float FrontSplitterDownforce { get; }
        public float RearWingDownforce { get; }

        public AeroDebugData(float frontSplitterDownforce, float rearWingDownforce) : this()
        {
            FrontSplitterDownforce = frontSplitterDownforce;
            RearWingDownforce = rearWingDownforce;
        }
    }
}