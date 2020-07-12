using Unity.Entities;

namespace TrafficSimulation.Components
{
    [GenerateAuthoringComponent]
    public struct VehicleComponent : IComponentData
    {
        public float Speed;
        public float CurrentSegPos;
    }
}