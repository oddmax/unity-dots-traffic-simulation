using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    public struct SegmentAddBlockLengthComponent : IComponentData
    {
        public float blockedLength;
    }
}