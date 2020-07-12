using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    [GenerateAuthoringComponent]
    public struct SegmentConfigComponent : IComponentData
    {
        public Entity StartNode;
        public Entity EndNode;
        public float Length;
    }
}