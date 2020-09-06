using TrafficSimulation.Components;
using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Systems
{
    public class VehicleMovementSystem : SystemBase
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
            var time = Time.DeltaTime;
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();
            
            BufferFromEntity<ConnectedSegmentBufferElement> nodeConnectedSegmentsBuffer = GetBufferFromEntity<ConnectedSegmentBufferElement>(true);
            
            var randomArray = World.GetExistingSystem<RandomSystem>().RandomArray;

            Entities.WithNativeDisableParallelForRestriction(randomArray)
                .WithNativeDisableParallelForRestriction(nodeConnectedSegmentsBuffer)
                .ForEach((Entity entity, int entityInQueryIndex, 
                    ref VehicleSegmentInfoComponent vehicleSegmentInfoComponent, 
                    ref VehicleComponent vehicleComponent, 
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    float frameSpeed = vehicleConfigComponent.Speed * time;
                    var newVehicleComponent = vehicleComponent;
                    var currentSegPos = newVehicleComponent.CurrentSegPos;
                    var segmentComponent = GetComponent<SegmentComponent>(vehicleSegmentInfoComponent.Segment);
                    if(currentSegPos >= segmentComponent.AvailableLength)
                    {
                        var valueToAddToAvailableLength = vehicleConfigComponent.Length;
                        var segmentConfigComponent = GetComponent<SegmentConfigComponent>(vehicleSegmentInfoComponent.Segment);
                        if (segmentComponent.AvailableLength >= segmentConfigComponent.Length)
                        {
                            if(!nodeConnectedSegmentsBuffer.Exists(vehicleSegmentInfoComponent.NextNode))
                            return;
                        
                            DynamicBuffer<ConnectedSegmentBufferElement> connectedSegmentBufferElements = 
                                nodeConnectedSegmentsBuffer[vehicleSegmentInfoComponent.NextNode];
                            
                            if (connectedSegmentBufferElements.Length > 1)
                            {
                                var random = randomArray[entityInQueryIndex];
                                var index = random.NextInt(0, connectedSegmentBufferElements.Length);
                                randomArray[entityInQueryIndex] = random;

                                var nextSegmentEntity = connectedSegmentBufferElements[index].segment;
                                var nextSegmentConfigComponent = GetComponent<SegmentConfigComponent>(nextSegmentEntity);
                                var nextSegmentComponent = GetComponent<SegmentComponent>(nextSegmentEntity);

                                if (nextSegmentComponent.TrafficType == ConnectionTrafficType.NoEntrance)
                                {
                                    //valueToAddToAvailableLength == 0;
                                }
                                else
                                {
                                    currentSegPos = 0;

                                    vehicleSegmentInfoComponent = new VehicleSegmentInfoComponent
                                    {
                                        Segment = nextSegmentEntity,
                                        SegmentLength = nextSegmentConfigComponent.Length,
                                        NextNode = nextSegmentConfigComponent.EndNode
                                    };
                                }
                            }
                            else
                            {
                                ecb.DestroyEntity(entityInQueryIndex, entity);
                                return;
                            }
                        }
                        //SetComponent(vehicleSegmentInfoComponent.Segment, new SegmentAddBlockLengthComponent { blockedLength = vehicleConfigComponent.Length});
                    }
                    else
                    {
                        currentSegPos += frameSpeed;
                        if (currentSegPos >= vehicleSegmentInfoComponent.SegmentLength)
                            currentSegPos = vehicleSegmentInfoComponent.SegmentLength;
                    }

                    newVehicleComponent.CurrentSegPos = currentSegPos;
                    vehicleComponent = newVehicleComponent;

                }).ScheduleParallel();
            
            endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

    }
}