using Unity.Entities;

namespace TrafficSimulation.Components.Vehicle
{
    [GenerateAuthoringComponent]
    public struct VehicleComponent : IComponentData
    {
        public float HeadSegPos;
        public float BackSegPos;
        public Entity FrontSegment;
        public Entity BackSegment;
    }
}