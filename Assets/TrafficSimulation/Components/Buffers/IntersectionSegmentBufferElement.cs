using Unity.Entities;

namespace TrafficSimulation.Components.Buffers
{
    [InternalBufferCapacity(ComponentConstants.MaxIntersectionSegments)]
    public struct IntersectionSegmentBufferElement : IBufferElementData
    {
        public Entity Segment;
    }
}