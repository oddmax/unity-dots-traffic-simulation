using Unity.Entities;

namespace TrafficSimulation.Components.Buffers
{
    [InternalBufferCapacity(4)]
    public struct ConnectedSegmentBufferElement : IBufferElementData
    {
        public Entity segment;
    }
}