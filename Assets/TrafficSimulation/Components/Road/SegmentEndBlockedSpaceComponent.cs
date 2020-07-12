using Unity.Entities;

namespace TrafficSimulation.Components
{
    public struct SegmentEndBlockedSpaceComponent : IComponentData
    {
        public float BlockedSegment;
    }
}