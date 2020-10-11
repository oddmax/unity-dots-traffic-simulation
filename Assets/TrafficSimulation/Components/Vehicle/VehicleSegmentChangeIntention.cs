using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleSegmentChangeIntention : IComponentData
    {
        public Entity NextSegment;
    }
}