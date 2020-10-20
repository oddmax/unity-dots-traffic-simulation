using Unity.Entities;

namespace TrafficSimulation.Systems
{
    [UpdateAfter(typeof(CalculateCarsInSegmentsSystem))]
    [UpdateInGroup(typeof(TrafficSimulationGroup))]
    public class SyncPointSystem : EntityCommandBufferSystem
    {
        
    }
    
    ///HashMapSystem
    /// -> SyncPointSystem
    /// MovementSystem
    /// IntersectionSystem
}