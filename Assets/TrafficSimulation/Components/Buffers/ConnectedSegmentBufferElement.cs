using Unity.Entities;

namespace TrafficSimulation.Components.Buffers
{
    public struct ConnectedSegmentBufferElement : IBufferElementData
    {
        public Entity segment;
    }
}