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
                    if(currentSegPos >= vehicleSegmentInfoComponent.SegmentLength)
                    {
                        //ecb.AddComponent(entityInQueryIndex, entity, new HasReachedNodeComponent());

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
                        var nextSegmentComponent = GetComponent<SegmentConfigComponent>(nextSegmentEntity);
                        currentSegPos = 0;

                        vehicleSegmentInfoComponent = new VehicleSegmentInfoComponent
                        {
                            Segment = nextSegmentEntity,
                            SegmentLength = nextSegmentComponent.Length,
                            NextNode = nextSegmentComponent.EndNode
                        };
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