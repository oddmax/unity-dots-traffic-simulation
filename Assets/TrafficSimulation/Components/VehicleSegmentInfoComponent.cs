using Unity.Entities;

namespace TrafficSimulation.Components
{
    [GenerateAuthoringComponent]
    public struct VehicleSegmentInfoComponent : IComponentData
    {
        public Entity Segment;
        public Entity NextNode;
    }
}