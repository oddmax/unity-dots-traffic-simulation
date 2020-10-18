using Unity.Entities;

namespace TrafficSimulation.Components.Intersection
{
    public struct IntersectionComponent : IComponentData
    {
        public int CurrentGroupIndex;
        public IntersectionPhaseType CurrentPhase;
    }
}