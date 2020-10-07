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

            Entities.ForEach((Entity entity, int entityInQueryIndex,
                    ref VehicleComponent vehicleComponent,
                    in VehicleSegmentInfoComponent vehicleSegmentInfoComponent,
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    float frameSpeed = vehicleConfigComponent.Speed * time;
                    var newVehicleComponent = vehicleComponent;
                    var currentSegPos = newVehicleComponent.HeadSegPos;
                    
                    var segmentComponent = GetComponent<SegmentComponent>(vehicleSegmentInfoComponent.HeadSegment);
                    currentSegPos = math.min(newVehicleComponent.HeadSegPos + frameSpeed, segmentComponent.AvailableLength);

                    newVehicleComponent.HeadSegPos = currentSegPos;
                    vehicleComponent = newVehicleComponent;
                }).ScheduleParallel();
            

            Entities.WithNativeDisableParallelForRestriction(randomArray)
                //.WithNativeDisableParallelForRestriction(nodeConnectedSegmentsBuffer)
                .ForEach((Entity entity, int entityInQueryIndex, 
                    ref VehicleSegmentInfoComponent vehicleSegmentInfoComponent, 
                    ref VehicleComponent vehicleComponent, 
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    var segment = vehicleSegmentInfoComponent.HeadSegment;
                    var segmentComponent = GetComponent<SegmentComponent>(segment);
                    var segmentComponentAvailableLength = segmentComponent.AvailableLength;
                    if(vehicleComponent.HeadSegPos >= segmentComponentAvailableLength)
                    {
                        var valueToAddToAvailableLength = 0f;
                        var segmentConfigComponent = GetComponent<SegmentConfigComponent>(segment);
                        if (vehicleComponent.HeadSegPos >= segmentConfigComponent.Length)
                        {
                            var isNextNodeExist = nodeConnectedSegmentsBuffer.Exists(vehicleSegmentInfoComponent.NextNode);
                            if (isNextNodeExist)
                            {
                                DynamicBuffer<ConnectedSegmentBufferElement> connectedSegmentBufferElements = 
                                    nodeConnectedSegmentsBuffer[vehicleSegmentInfoComponent.NextNode];
                            
                                if (connectedSegmentBufferElements.Length > 0)
                                {
                                    var random = randomArray[entityInQueryIndex];
                                    var index = random.NextInt(0, connectedSegmentBufferElements.Length);
                                    randomArray[entityInQueryIndex] = random;

                                    var nextSegmentEntity = connectedSegmentBufferElements[index].segment;
                                    var nextSegmentConfigComponent = GetComponent<SegmentConfigComponent>(nextSegmentEntity);
                                    var nextSegmentComponent = GetComponent<SegmentComponent>(nextSegmentEntity);

                                    if (nextSegmentComponent.TrafficType == ConnectionTrafficType.NoEntrance)
                                    {
                                        valueToAddToAvailableLength = -vehicleConfigComponent.Length;
                                    }
                                    else
                                    {
                                        vehicleSegmentInfoComponent = new VehicleSegmentInfoComponent
                                        {
                                            HeadSegment = nextSegmentEntity,
                                            SegmentLength = nextSegmentConfigComponent.Length,
                                            NextNode = nextSegmentConfigComponent.EndNode
                                        };
                                        valueToAddToAvailableLength = vehicleConfigComponent.Length;
                                        vehicleComponent = new VehicleComponent
                                        {
                                            HeadSegPos = 0
                                        };
                                    }

                                    isNextNodeExist = true;
                                }  
                            }
                            
                            else
                            {
                                ecb.DestroyEntity(entityInQueryIndex, entity);
                                return;
                            }
                        } 
                        SetComponent(segment, new SegmentComponent { AvailableLength = segmentComponentAvailableLength + valueToAddToAvailableLength});
                       // SetComponent(vehicleSegmentInfoComponent.Segment, new SegmentAddBlockLengthComponent { blockedLength = vehicleConfigComponent.Length});
                    }

                }).Schedule();
            
            endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

    }
}