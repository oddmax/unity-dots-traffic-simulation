using Unity.Entities;

namespace TrafficSimulation.Components
{
    public struct VehicleTarget : IComponentData
    {
        public Entity Segment;
        public Entity NextNode;
    }
}