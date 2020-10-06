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
        public static NativeMultiHashMap<Entity, VehicleSegmentData> MultiHashMap;
        
        protected override void OnCreate() {
            MultiHashMap = new NativeMultiHashMap<Entity, VehicleSegmentData>(0, Allocator.Persistent);
            base.OnCreate();
        }

        protected override void OnDestroy() {
            MultiHashMap.Dispose();
            base.OnDestroy();
        }
        
        protected override void OnUpdate()
        {
            MultiHashMap.Clear();
            EntityQuery entityQuery = GetEntityQuery(typeof(VehicleComponent));
            if (entityQuery.CalculateEntityCount() > MultiHashMap.Capacity) {
                MultiHashMap.Capacity = entityQuery.CalculateEntityCount();
            }
            
            NativeMultiHashMap<Entity, VehicleSegmentData>.ParallelWriter multiHashMap = MultiHashMap.AsParallelWriter();
            Entities.ForEach((Entity entity, int entityInQueryIndex,
                in VehicleComponent vehicleComponent) =>
            {
                multiHashMap.Add(vehicleComponent.BackSegment, new VehicleSegmentData
                {
                    Entity = entity,
                    BackSegPosition = vehicleComponent.BackSegPos
                });
            }).Run();
        }
    }
}