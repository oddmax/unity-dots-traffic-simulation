using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    [GenerateAuthoringComponent]
    public struct SegmentComponent : IComponentData
    {
        public float AvailableLength;
        public ConnectionTrafficType TrafficType;
    }
}