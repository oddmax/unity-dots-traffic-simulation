using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Components
{
    [GenerateAuthoringComponent]
    public struct SegmentComponent : IComponentData
    {
        public Entity StartNode;
        public Entity EndNode;
        public float3 MovementDirection;
        public float Length;
        public int MaxSpeed;
    }
}