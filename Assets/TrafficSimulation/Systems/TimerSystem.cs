using TrafficSimulation.Components.General;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    /// <summary>
    /// Simple system to decrease timer time count
    /// </summary>
    [UpdateInGroup(typeof(TrafficSimulationGroup))]
    [UpdateAfter(typeof(VehicleMovementSystem))]
    public class TimerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            
            Entities
                .ForEach((Entity entity, int entityInQueryIndex,
                    ref TimerComponent timerComponent) =>
                {
                    if(timerComponent.IsTimerOver == true)
                        return;

                    timerComponent.TimeLeft -= time;
                    if (timerComponent.TimeLeft <= 0)
                    {
                        timerComponent.TimeLeft = 0;
                        timerComponent.IsTimerOver = true;
                    }
                    
                }).ScheduleParallel();
                
        }
    }
}