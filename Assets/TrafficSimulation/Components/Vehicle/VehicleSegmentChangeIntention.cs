using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    public struct VehicleSegmentChangeIntention : IComponentData
    {
        public Entity NextFrontSegment;
        public Entity NextBackSegment;
    }
}