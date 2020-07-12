using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleConfigComponent : IComponentData
    {
        public float Speed;
        public float Length;
    }
}