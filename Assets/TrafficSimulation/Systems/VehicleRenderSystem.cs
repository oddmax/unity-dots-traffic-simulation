using TrafficSimulation.Components;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TrafficSimulation.Systems
{
    public class VehicleRenderSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation,
                    in VehicleSegmentInfoComponent vehicleSegmentInfoComponent, in VehiclePositionComponent vehicleComponent) =>
                {
                    var oldTranslation = translation;
                    var spline = GetComponent<SplineComponent>(vehicleSegmentInfoComponent.HeadSegment);
                    var length = spline.Length;

                    var newTrans = new Translation {Value = spline.Point(vehicleComponent.HeadSegPos / length)};

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