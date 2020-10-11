using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleSegmentInfoComponent : IComponentData
    {
        public Entity HeadSegment;
        public float SegmentLength;
        
        public bool IsBackInPreviousSegment;
        public Entity PreviousSegment;
        public float PreviousSegmentLength;
        
        public Entity NextNode;
    }
}