using TrafficSimulation.Components.General;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    /// <summary>
    /// Simple system to decrease timer time count
    /// </summary>
    [UpdateInGroup(typeof(TrafficSimulationGroup))]
    [UpdateAfter(typeof(VehicleMovementSystem))]
    public class RemoveVehiclesOnTimer : SystemBase
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override void OnUpdate()
        {
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
            
            Entities
                .WithAll<VehicleConfigComponent>()
                .ForEach((Entity entity, int entityInQueryIndex, in TimerComponent timerComponent) =>
                {
                    if(timerComponent.IsTimerOver == true)
                        ecb.DestroyEntity(entityInQueryIndex, entity);
                    
                }).ScheduleParallel();
                
            endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }
    }
}