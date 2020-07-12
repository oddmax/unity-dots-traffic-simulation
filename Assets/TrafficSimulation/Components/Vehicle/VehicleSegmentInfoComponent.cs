using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleSegmentInfoComponent : IComponentData
    {
        public Entity Segment;
        public float SegmentLength;
        public Entity NextNode;
    }
}