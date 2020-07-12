using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleComponent : IComponentData
    {
        public float CurrentSegPos;
    }
}