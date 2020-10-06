using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    public struct SegmentLastVehicleInSegmentInfo : IComponentData
    {
        public Entity lastVehicleEntity;
        public float lastVehilceEntityBackPosition;
    }
}