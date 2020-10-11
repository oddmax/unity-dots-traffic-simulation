using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TrafficSimulation.Systems
{
    public class VehicleRenderSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((Entity entity, int entityInQueryIndex, 
                    ref Translation translation, 
                    ref Rotation rotation,
                    in VehicleSegmentInfoComponent vehicleSegmentInfoComponent, 
                    in VehiclePositionComponent vehicleComponent,
                    in VehicleConfigComponent vehicleConfigComponent) =>
                {
                    var oldTranslation = translation;
                    var centerVehiclePos = vehicleComponent.HeadSegPos - vehicleConfigComponent.Length / 2;
                    var centerSegment = centerVehiclePos >= 0f
                        ? vehicleSegmentInfoComponent.HeadSegment
                        : vehicleSegmentInfoComponent.PreviousSegment;

                    if (centerVehiclePos < 0)
                    {
                        centerVehiclePos += vehicleSegmentInfoComponent.PreviousSegmentLength;
                    }
                    
                    var spline = GetComponent<SplineComponent>(centerSegment);
                    var length = spline.Length;

                    var newTrans = new Translation {Value = spline.Point(centerVehiclePos / length)};

                    var translationChange = oldTranslation.Value - newTrans.Value;
                    var newRotation = new Rotation
                        {Value = quaternion.LookRotation(translationChange, math.up())};
                    
                    translation = newTrans;
                    if(!translationChange.Equals(float3.zero))
                        rotation = newRotation;
                    
                }).ScheduleParallel();
        }
    }
}