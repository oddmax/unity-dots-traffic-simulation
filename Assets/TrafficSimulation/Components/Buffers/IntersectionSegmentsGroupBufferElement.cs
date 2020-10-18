using Unity.Entities;

namespace TrafficSimulation.Components.Buffers
{
    [InternalBufferCapacity(ComponentConstants.MaxIntersectionGroups)]
    public struct IntersectionSegmentsGroupBufferElement : IBufferElementData
    {
        public int StartIndex;
        public int EndIndex;
        public int Time;
    }
}