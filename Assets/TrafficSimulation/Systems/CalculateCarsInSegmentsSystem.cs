using TrafficSimulation.Components.Vehicle;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    public struct VehicleSegmentData
    {
        public Entity Entity;
        public float BackSegPosition;
        public float VehicleSize;
    }
    
    /// <summary>
    /// Calculates cars inside of the segments in the beginning of the frame and adds them to NativeMultiHashMap
    /// later access
    /// </summary>
    [UpdateInGroup(typeof(TrafficSimulationGroup))]
    public class CalculateCarsInSegmentsSystem : SystemBase
    {
        public static NativeMultiHashMap<Entity, VehicleSegmentData> VehiclesSegmentsHashMap;
        
        protected override void OnCreate() {
            VehiclesSegmentsHashMap = new NativeMultiHashMap<Entity, VehicleSegmentData>(0, Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy() {
            VehiclesSegmentsHashMap.Dispose();
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            VehiclesSegmentsHashMap.Clear();
            EntityQuery entityQuery = GetEntityQuery(typeof(VehiclePositionComponent));
            if (entityQuery.CalculateEntityCount() > VehiclesSegmentsHashMap.Capacity) {
                VehiclesSegmentsHashMap.Capacity = entityQuery.CalculateEntityCount();
            }
            
            NativeMultiHashMap<Entity, VehicleSegmentData>.ParallelWriter multiHashMap = VehiclesSegmentsHashMap.AsParallelWriter();
            var jobHandle = Entities.ForEach((Entity entity, int entityInQueryIndex,
                in VehicleSegmentInfoComponent vehicleSegmentInfoComponent,
                in VehiclePositionComponent vehiclePositionComponent,
                in VehicleConfigComponent vehicleConfigComponent) =>
            {
                Entity segmentEntity = vehicleSegmentInfoComponent.IsBackInPreviousSegment
                    ? vehicleSegmentInfoComponent.PreviousSegment
                    : vehicleSegmentInfoComponent.HeadSegment;
                multiHashMap.Add(segmentEntity, new VehicleSegmentData
                {
                    Entity = entity,
                    BackSegPosition = vehiclePositionComponent.BackSegPos,
                    VehicleSize = vehicleConfigComponent.Length
                });
            }).Schedule(this.Dependency);
            jobHandle.Complete();
        }
    }
}