using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleMoveIntention : IComponentData
    {
        public float AvailableDistance;
    }
}