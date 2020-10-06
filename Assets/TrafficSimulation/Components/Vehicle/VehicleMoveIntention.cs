using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    public struct VehicleMoveIntention : IComponentData
    {
        public float AvailableDistance;
    }
}