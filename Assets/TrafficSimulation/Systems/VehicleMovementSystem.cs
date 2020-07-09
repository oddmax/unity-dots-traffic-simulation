using TrafficSimulation.Components;
using TrafficSimulation.Components.Buffers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TrafficSimulation.Systems
{
    public class VehicleMovementSystem : SystemBase
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
        
        protected override void OnUpdate()
        {
            var time = Time.DeltaTime;
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
            
            BufferFromEntity<ConnectedSegmentBufferElement> nodeConnectedSegmentsBuffer = GetBufferFromEntity<ConnectedSegmentBufferElement>(true);
            
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities.WithNativeDisableParallelForRestriction(randomArray)
                .WithNativeDisableParallelForRestriction(nodeConnectedSegmentsBuffer)
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation,
                   ref VehicleSegmentInfoComponent vehicleSegmentInfoComponent, in VehicleComponent vehicleComponent) =>
                {
                    var newTrans = translation;
                    var newRotation = rotation;

                    float3 target = GetComponent<RoadNodeComponent>(vehicleSegmentInfoComponent.NextNode).Position;
                    float3 delta = target - newTrans.Value;
                    float frameSpeed = vehicleComponent.Speed * time;

                    if (math.length(delta) < frameSpeed)
                    {
                        newTrans.Value = target;
                        ecb.AddComponent(entityInQueryIndex, entity, new HasReachedNodeComponent());

                        if(!nodeConnectedSegmentsBuffer.Exists(vehicleSegmentInfoComponent.NextNode))
                            return;
                        
                        DynamicBuffer<ConnectedSegmentBufferElement> connectedSegmentBufferElements = 
                            nodeConnectedSegmentsBuffer[vehicleSegmentInfoComponent.NextNode];
                        
                        var index = 0;
                        if (connectedSegmentBufferElements.Length > 1)
                        {
                            var random = randomArray[entityInQueryIndex];
                            index = random.NextInt(0, connectedSegmentBufferElements.Length);
                            randomArray[entityInQueryIndex] = random;
                        }
                        var nextSegmentEntity = connectedSegmentBufferElements[index].segment;
                            
                        vehicleSegmentInfoComponent = new VehicleSegmentInfoComponent
                        {
                            Segment = nextSegmentEntity,
                            NextNode = GetComponent<SegmentComponent>(nextSegmentEntity).EndNode
                        };
                    }
                    else
                    {
                        newTrans.Value += math.normalize(delta) * frameSpeed;
                        newRotation.Value = quaternion.LookRotation(delta, math.up());
                    }

                    rotation = newRotation;
                    translation = newTrans;

                }).ScheduleParallel();
            
            endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

        private struct ChooseNextSegmentJob : JobForEachExtensions.IBaseJobForEach
        {
        }
        
    }
}