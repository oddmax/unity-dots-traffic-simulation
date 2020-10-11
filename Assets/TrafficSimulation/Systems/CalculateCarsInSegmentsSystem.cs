using System;
using TrafficSimulation.Components.Vehicle;
using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    public struct VehicleSegmentData
    {
        public Entity Entity;
        public float BackSegPosition;
    }
    
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
            Entities.ForEach((Entity entity, int entityInQueryIndex,
                in VehicleSegmentInfoComponent vehicleSegmentInfoComponent,
                in VehiclePositionComponent vehiclePositionComponent) =>
            {
                multiHashMap.Add(vehicleSegmentInfoComponent.BackSegment, new VehicleSegmentData
                {
                    Entity = entity,
                    BackSegPosition = vehiclePositionComponent.BackSegPos
                });
            }).Run();
        }
    }
}