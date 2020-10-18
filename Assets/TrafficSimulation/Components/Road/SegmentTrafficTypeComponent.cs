using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    [GenerateAuthoringComponent]
    public struct SegmentTrafficTypeComponent : IComponentData
    {
        public ConnectionTrafficType TrafficType;
    }
}