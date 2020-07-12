using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    public struct SegmentEndBlockedSpaceComponent : IComponentData
    {
        public float BlockedSegment;
    }
}