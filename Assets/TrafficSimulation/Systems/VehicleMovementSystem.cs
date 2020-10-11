using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Systems
{
    [UpdateAfter(typeof(CalculateCarsInSegmentsSystem))]
    public class VehicleMovementSystem : SystemBase
    {
        private const float MIN_DISTANCE = 0.2f;
        private const float MAX_DISTANCE = 2f;
        private const float ACCELERATION = 0.2f;
        
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
            
            //move job - updates positions of the vehicle
            Entities.ForEach((Entity entity, int entityInQueryIndex,
                    ref VehiclePositionComponent vehicleComponent,
                    ref VehicleSegmentInfoComponent vehicleSegmentInfoComponent,
                    in VehicleSegmentChangeIntention segmentChangeIntention,
                    in VehicleVelocityComponent velocityComponent,
                    in VehicleMoveIntention moveIntention) =>
                {
                    float frameSpeed = velocityComponent.Value * time;
                    var newHeadSegPos = vehicleComponent.HeadSegPos;
                    var newVehicleComponent = vehicleComponent;
                    var newVehicleSegmentInfoComponent = vehicleSegmentInfoComponent;

                    newHeadSegPos = math.min(vehicleComponent.HeadSegPos + frameSpeed, vehicleComponent.HeadSegPos + moveIntention.AvailableDistance);
                    var changeDist = newHeadSegPos - vehicleComponent.HeadSegPos;
                    var newBackSegPos = vehicleComponent.BackSegPos + changeDist;

                    if (newHeadSegPos > vehicleSegmentInfoComponent.SegmentLength)
                    {
                        newHeadSegPos = newHeadSegPos - vehicleSegmentInfoComponent.SegmentLength;
                        newVehicleSegmentInfoComponent.HeadSegment = segmentChangeIntention.NextFrontSegment;
                    }

                    if (newBackSegPos > vehicleSegmentInfoComponent.SegmentLength)
                    {
                        newBackSegPos = newBackSegPos - vehicleSegmentInfoComponent.SegmentLength;
                        newVehicleSegmentInfoComponent.BackSegment = segmentChangeIntention.NextBackSegment;
                    }

                    // update position values
                    newVehicleComponent.HeadSegPos = newHeadSegPos;
                    newVehicleComponent.BackSegPos = newBackSegPos;
                    
                    vehicleComponent = newVehicleComponent;
                    vehicleSegmentInfoComponent = newVehicleSegmentInfoComponent;
                }).ScheduleParallel();
            
            
            //Put vehicle segment map into class variable, to use burst and jobs in dots you can't pass static fields.
            //Thanks to that multihashmap is a struct so only value is copied to local variable.
            //That variable is later used inside of the ForEach method.
            var vehiclesSegmentsHashMap = CalculateCarsInSegmentsSystem.VehiclesSegmentsHashMap;
            
            //calculate next frame move intention
            Entities.WithReadOnly(vehiclesSegmentsHashMap)
                .ForEach((Entity entity, int entityInQueryIndex,
                    ref VehicleMoveIntention moveIntention,
                    ref VehicleVelocityComponent velocityComponent,
                    in VehicleSegmentInfoComponent vehicleSegmentInfoComponent, 
                    in VehicleSegmentChangeIntention vehicleSegmentChangeIntention,
                    in VehiclePositionComponent vehicleComponent, 
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    var hashMapKey = new CarsInSegmentsFindHelper();
                   
                    var segment = vehicleSegmentInfoComponent.HeadSegment;
                    var nextVehicleInSegment = Entity.Null;
                    var nextVehicleInSegmentPosition = 0f;
                    var distance = 0f;
                    var headSegPos = vehicleComponent.HeadSegPos;
                    
                    // Calculate distance which is available in front of the vehicle
                    hashMapKey.FindVehicleInFrontInSegment(vehiclesSegmentsHashMap, segment, headSegPos, ref nextVehicleInSegment, ref nextVehicleInSegmentPosition);
                    //there is a car in front of current car, calculate distance between them
                    if (nextVehicleInSegment != Entity.Null)
                    {
                        distance = nextVehicleInSegmentPosition - headSegPos;
                    }
                    else
                    {
                        //there is no car in front in the same segment
                        var distanceTillNode = vehicleSegmentInfoComponent.SegmentLength - headSegPos;
                        distance = distanceTillNode;
                        
                        if (distanceTillNode < MAX_DISTANCE && vehicleSegmentChangeIntention.NextFrontSegment != Entity.Null)
                        {
                            var nextSegmentComponent = GetComponent<SegmentComponent>(segment);
                            if (nextSegmentComponent.TrafficType == ConnectionTrafficType.Normal)
                            {
                                distance += nextSegmentComponent.AvailableLength;
                            }
                        }
                    }
                    
                    //Calculate new velocity based on available distance
                    var newVelocity = CalculateVelocityBasedOnDistance(velocityComponent.Value, vehicleConfigComponent.Speed, distance);
                    velocityComponent = new VehicleVelocityComponent {Value = newVelocity};
                    moveIntention = new VehicleMoveIntention {AvailableDistance = distance};
                }).Schedule();
            endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
        }

        public static float CalculateVelocityBasedOnDistance(float currentVelocity, float maxSpeed, float distance)
        {
            float velocity = 0;
            if (distance > MAX_DISTANCE) 
                velocity += ACCELERATION;
            else if (MIN_DISTANCE < distance && distance <= MAX_DISTANCE)
                velocity -= ACCELERATION;
            else if (distance <= MIN_DISTANCE)
                velocity = 0f;
            
            return velocity;
        }

        
        /*var isNextNodeExist = nodeConnectedSegmentsBuffer.Exists(vehicleSegmentInfoComponent.NextNode);
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
        }*/
    }
}