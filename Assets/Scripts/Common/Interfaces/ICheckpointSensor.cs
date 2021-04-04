using Common.Components;
using Common.Components.TrackParts;

namespace Common.Interfaces
{
    public interface ICheckpointSensor
    {
        void Init(Checkpoint checkpoint);
        void Report(Transponder transponder);
    }
}