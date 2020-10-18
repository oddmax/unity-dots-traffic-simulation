using Unity.Entities;

namespace TrafficSimulation.Components.Buffers
{
    [InternalBufferCapacity(ComponentConstants.MaxSegmentsConnectedToOneNode)]
    public struct ConnectedSegmentBufferElement : IBufferElementData
    {
        public Entity segment;
    }
}