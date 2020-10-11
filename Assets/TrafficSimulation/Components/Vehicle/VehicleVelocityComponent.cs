using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleVelocityComponent : IComponentData
    {
        public float Value;
    }
}