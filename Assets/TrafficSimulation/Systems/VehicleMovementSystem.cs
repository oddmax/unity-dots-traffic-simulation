using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;
using Unity.Mathematics;

namespace TrafficSimulation.Systems
{
    
    /// <summary>
    /// Main system which update vehicles positions on the road network, keeps distance from the front cars and blocked segments
    /// and randomly chooses next segment to go
    /// </summary>
    [UpdateInGroup(typeof(TrafficSimulationGroup))]
    [UpdateAfter(typeof(SyncPointSystem))]
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
            Entities
                .WithReadOnly(nodeConnectedSegmentsBuffer)
                .ForEach((Entity entity, int entityInQueryIndex,
                    ref VehiclePositionComponent vehicleComponent,
                    ref VehicleSegmentInfoComponent vehicleSegmentInfoComponent,
                    ref VehicleSegmentChangeIntention segmentChangeIntention,
                    in VehicleVelocityComponent velocityComponent,
                    in VehicleMoveIntention moveIntention,
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    if(moveIntention.AvailableDistance == 0)
                        return;
                    
                    float frameSpeed = velocityComponent.Value * time;
                    var newHeadSegPos = vehicleComponent.HeadSegPos;
                    var newVehicleComponent = vehicleComponent;
                    var newVehicleSegmentInfoComponent = vehicleSegmentInfoComponent;

                    newHeadSegPos = math.min(vehicleComponent.HeadSegPos + frameSpeed, vehicleComponent.HeadSegPos + moveIntention.AvailableDistance);
                    
                    if (newHeadSegPos >= vehicleSegmentInfoComponent.SegmentLength)
                    {
                        // choosing next random segment after the current one
                        var newSegmentChangeIntention = segmentChangeIntention;
                        var newSegmentConfig = GetComponent<SegmentConfigComponent>(segmentChangeIntention.NextSegment);
                        
                        // set new segment information
                        newVehicleSegmentInfoComponent.HeadSegment = segmentChangeIntention.NextSegment;
                        newVehicleSegmentInfoComponent.NextNode = newSegmentConfig.EndNode;
                        newVehicleSegmentInfoComponent.SegmentLength = newSegmentConfig.Length;
                        newVehicleSegmentInfoComponent.PreviousSegment = vehicleSegmentInfoComponent.HeadSegment;
                        newVehicleSegmentInfoComponent.PreviousSegmentLength = vehicleSegmentInfoComponent.SegmentLength;
                        newVehicleSegmentInfoComponent.IsBackInPreviousSegment = true;
                        newHeadSegPos = newHeadSegPos - vehicleSegmentInfoComponent.SegmentLength;
                        
                        // choose segment to go after the new one
                        newSegmentChangeIntention.NextSegment = Entity.Null;
                        if (nodeConnectedSegmentsBuffer.Exists(newSegmentConfig.EndNode))
                        {
                            DynamicBuffer<ConnectedSegmentBufferElement> connectedSegmentBufferElements = nodeConnectedSegmentsBuffer[newSegmentConfig.EndNode];
                            if (connectedSegmentBufferElements.Length > 0)
                            {
                                var random = randomArray[entityInQueryIndex];
                                var index = random.NextInt(0, connectedSegmentBufferElements.Length);
                                randomArray[entityInQueryIndex] = random;

                                var nextSegmentEntity = connectedSegmentBufferElements[index].segment;
                                newSegmentChangeIntention.NextSegment = nextSegmentEntity;
                            }  
                        }
                        segmentChangeIntention = newSegmentChangeIntention;
                    }

                    float newBackSegPos = newHeadSegPos - vehicleConfigComponent.Length;
                    if (newVehicleSegmentInfoComponent.IsBackInPreviousSegment)
                    {
                        if (newBackSegPos >= 0)
                        {
                            newVehicleSegmentInfoComponent.IsBackInPreviousSegment = false;
                            newVehicleSegmentInfoComponent.PreviousSegment = Entity.Null;
                        }
                        else
                        {
                            newBackSegPos += vehicleSegmentInfoComponent.PreviousSegmentLength;
                        }
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

            Entities
                .WithReadOnly(vehiclesSegmentsHashMap)
                .ForEach((Entity entity, int entityInQueryIndex, 
                        ref SegmentComponent segmentComponent,
                        in SegmentConfigComponent segmentConfigComponent) =>
                {
                    var nextVehicleInSegment = Entity.Null;
                    var nextVehicleInSegmentPosition = 0f;
                    var newSegmentComponent = segmentComponent;
                    
                    var hashMapKeyHelper = new VehiclesInSegmentHashMapHelper();
                    // Calculate distance which is available in front of the vehicle
                    hashMapKeyHelper.FindVehicleInFrontInSegment(vehiclesSegmentsHashMap, entity, 0, ref nextVehicleInSegment, ref nextVehicleInSegmentPosition);
                    newSegmentComponent.AvailableLength = nextVehicleInSegment == Entity.Null ? segmentConfigComponent.Length : nextVehicleInSegmentPosition;

                    segmentComponent = newSegmentComponent;
                }).Schedule();

            
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
                    var hashMapKey = new VehiclesInSegmentHashMapHelper();
                   
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
                        
                        if (distanceTillNode < MAX_DISTANCE && vehicleSegmentChangeIntention.NextSegment != Entity.Null)
                        {
                            var nexSegmentTrafficTypeComp = GetComponent<SegmentTrafficTypeComponent>(vehicleSegmentChangeIntention.NextSegment);
                            var nexSegmentComp = GetComponent<SegmentComponent>(vehicleSegmentChangeIntention.NextSegment);
                            if (nexSegmentTrafficTypeComp.TrafficType == ConnectionTrafficType.Normal)
                                distance += nexSegmentComp.AvailableLength;
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
            float velocity = currentVelocity;
            if (distance > MAX_DISTANCE) 
                velocity += ACCELERATION;
            else if (MIN_DISTANCE < distance && distance <= MAX_DISTANCE)
                velocity -= ACCELERATION/5;
            else if (distance <= MIN_DISTANCE)
                velocity = 0f;

            if (velocity > maxSpeed)
                velocity = maxSpeed;

            if (velocity < 0f)
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