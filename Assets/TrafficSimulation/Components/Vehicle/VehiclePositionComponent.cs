using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehiclePositionComponent : IComponentData
    {
        public float HeadSegPos;
        public float BackSegPos;
    }
}