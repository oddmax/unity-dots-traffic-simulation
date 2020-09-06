using TrafficSimulation.Components.Road;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    [UpdateAfter(typeof(VehicleMovementSystem))]
    public class CalculateAvailableSpaceSegmentsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, int entityInQueryIndex,
                ref SegmentComponent segmentComponent,
                in SegmentAddBlockLengthComponent segmentAddBlockLengthComponent,
                in SegmentConfigComponent segmentConfigComponent) =>
            {
                segmentComponent.AvailableLength += segmentAddBlockLengthComponent.blockedLength;
            }).Schedule();
        }
    }
}