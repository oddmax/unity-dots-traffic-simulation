using TrafficSimulation.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace TrafficSimulation.Systems
{
    public class VehicleMovementSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            // Find the ECB system once and store it for later usage
            //TODO what is EntityCommandBuffer System? 
            endSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var time = Time.DeltaTime;
            
            //TODO find out why name "UnitDenormalizedJob"?
            //TODO why translation is passed by 'ref' and custom component by 'in'
            var vehicleJob = Entities.WithName("UnitDenormalizedJob")
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation,
                    in VehicleComponent vehicleComponent) =>
                {
                    var newTrans = translation;

                    float3 target = vehicleComponent.Target;
                    float3 delta = target - newTrans.Value;
                    float frameSpeed = vehicleComponent.Speed * time;

                    if (math.length(delta) < frameSpeed)
                    {
                        newTrans.Value = target;
                    }
                    else
                    {
                        newTrans.Value += math.normalize(delta) * frameSpeed;
                    }

                    translation = newTrans;

                }).Schedule(inputDeps);

            return vehicleJob;
        }
    }
}