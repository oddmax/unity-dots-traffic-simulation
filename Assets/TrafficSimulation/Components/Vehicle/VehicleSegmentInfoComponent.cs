using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleSegmentInfoComponent : IComponentData
    {
        public Entity HeadSegment;
        public Entity BackSegment;
        public float SegmentLength;
        public float SegmentAvailableLength;
        public Entity NextNode;
    }
}