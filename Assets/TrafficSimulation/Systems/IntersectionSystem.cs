using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Intersection;
using TrafficSimulation.Components.Road;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    [UpdateAfter(typeof(CalculateCarsInSegmentsSystem))]
    public class IntersectionSystem : SystemBase
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
            BufferFromEntity<IntersectionSegmentsGroupBufferElement> intersectionGroups = GetBufferFromEntity<IntersectionSegmentsGroupBufferElement>(true);
            BufferFromEntity<IntersectionSegmentBufferElement> intersectionSegments = GetBufferFromEntity<IntersectionSegmentBufferElement>(true);
            var vehiclesSegmentsHashMap = CalculateCarsInSegmentsSystem.VehiclesSegmentsHashMap;

            Entities
                .WithReadOnly(vehiclesSegmentsHashMap)
                .WithReadOnly(intersectionGroups)
                .WithReadOnly(intersectionSegments)
                .ForEach((Entity entity, int entityInQueryIndex,
                    ref IntersectionTimerComponent intersectionTimerComponent,
                    ref IntersectionComponent intersectionComponent) =>
                {
                    //is time to change phase?
                    if(intersectionTimerComponent.FramesLeft != 0) 
                        return;
                    
                    var intersectionGroupsBufferElements = intersectionGroups[entity];
                    var intersectionSegmentsBufferElements = intersectionSegments[entity];
                    var currentGroup = intersectionGroupsBufferElements[intersectionComponent.CurrentGroupIndex];

                    switch (intersectionComponent.CurrentPhase)
                    {
                        //switch to phase of waiting to all traffic to leave the segments
                        case IntersectionPhaseType.PassThrough:
                            intersectionComponent.CurrentPhase = IntersectionPhaseType.ClearingTraffic;
                            //change all segments in current group to NoEntrance as new vehicles should not enter
                            for (int i = currentGroup.StartIndex; i <= currentGroup.EndIndex; i++)
                            {
                                var segmentEntity = intersectionSegmentsBufferElements[i].Segment;
                                SetComponent(segmentEntity, new SegmentTrafficTypeComponent
                                {
                                    TrafficType = ConnectionTrafficType.NoEntrance
                                });
                            }
                            break;
                        case IntersectionPhaseType.ClearingTraffic:
                            //check is all segments in current group are free of vehicles
                            var hashMapKeyHelper = new VehiclesInSegmentHashMapHelper();
                            bool isAllSegmentsAreEmpty = true;
                            for (int i = currentGroup.StartIndex; i <= currentGroup.EndIndex; i++)
                            {
                                var segmentEntity = intersectionSegmentsBufferElements[i].Segment;
                                if (hashMapKeyHelper.HasVehicleInSegment(vehiclesSegmentsHashMap, segmentEntity))
                                {
                                    isAllSegmentsAreEmpty = false;
                                    break;
                                }
                            }

                            //if all segments are empty we can switch group and turn next phase
                            if (isAllSegmentsAreEmpty)
                            {
                                var newIntersectionComponent = intersectionComponent;
                                var newIntersectionTimerComponent = intersectionTimerComponent;
                                newIntersectionComponent.CurrentPhase = IntersectionPhaseType.PassThrough;
                                newIntersectionComponent.CurrentGroupIndex =
                                    (intersectionComponent.CurrentGroupIndex + 1) %
                                    intersectionGroupsBufferElements.Length;

                                var nextGroup =
                                    intersectionGroupsBufferElements[intersectionComponent.CurrentGroupIndex];

                                newIntersectionTimerComponent.FramesLeft = nextGroup.Time;

                                //set next segments group to allow traffic
                                for (int i = currentGroup.StartIndex; i <= currentGroup.EndIndex; i++)
                                {
                                    var segmentEntity = intersectionSegmentsBufferElements[i].Segment;
                                    SetComponent(segmentEntity, new SegmentTrafficTypeComponent
                                    {
                                        TrafficType = ConnectionTrafficType.Normal
                                    });
                                }

                                intersectionComponent = newIntersectionComponent;
                                intersectionTimerComponent = newIntersectionTimerComponent;
                            }

                            break;
                    }

                }).ScheduleParallel();
        }
        
    }
}