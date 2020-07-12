using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleComponent : IComponentData
    {
        public float Speed;
        public float CurrentSegPos;
    }
}