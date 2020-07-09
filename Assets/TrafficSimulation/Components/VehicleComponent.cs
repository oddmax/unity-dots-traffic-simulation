using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Components
{
    [GenerateAuthoringComponent]
    public struct VehicleComponent : IComponentData
    {
        public float Speed;
        public float3 MovementDirection;
        public float3 Target;
        public float CurrentSegPos;
    }
}