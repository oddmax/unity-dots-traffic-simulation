using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Components
{
    [GenerateAuthoringComponent]
    public struct RoadNodeComponent : IComponentData
    {
        public float3 Position;
    }
}