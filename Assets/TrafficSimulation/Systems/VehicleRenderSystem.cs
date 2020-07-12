using Model.Components;
using TrafficSimulation.Components;
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
                .ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref Rotation rotation,
                    in VehicleSegmentInfoComponent vehicleSegmentInfoComponent, in VehicleComponent vehicleComponent) =>
                {
                    var oldTranslation = translation;
                    var spline = GetComponent<SplineComponent>(vehicleSegmentInfoComponent.Segment);
                    var length = spline.Length;
                    
                    var newTrans = new Translation
                        {Value = spline.Point(vehicleComponent.CurrentSegPos / length)};

                    var newRotation = new Rotation
                        {Value = quaternion.LookRotation(oldTranslation.Value - newTrans.Value, math.up())};
                    
                    rotation = newRotation;
                    translation = newTrans;
                }).ScheduleParallel();
        }
    }
}