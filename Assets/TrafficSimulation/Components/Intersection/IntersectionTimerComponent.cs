using Unity.Entities;

namespace TrafficSimulation.Components.Intersection
{
    public struct IntersectionTimerComponent : IComponentData
    {
        public int FramesLeft;
    }
}