using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    public struct VehiclesInSegmentHashMapHelper
    {
        public void FindVehicleInFrontInSegment(
            NativeMultiHashMap<Entity, VehicleSegmentData> vehicleSegmentMap,
            Entity segmentEntity,
            float vehicleHeadPosition,
            ref Entity nextVehicleEntity,
            ref float nextVehicleBackPosition
        )
        {
            NativeMultiHashMapIterator<Entity> nativeMultiHashMapIterator;
            if (vehicleSegmentMap.TryGetFirstValue(segmentEntity, out var segmentData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (!(vehicleHeadPosition < segmentData.BackSegPosition)) 
                        continue;
                    
                    if (nextVehicleEntity == Entity.Null)
                    {
                        //no next vehicle, assign
                        nextVehicleEntity = segmentData.Entity;
                        nextVehicleBackPosition = segmentData.BackSegPosition;
                    }
                    else
                    {
                        if (segmentData.BackSegPosition < nextVehicleBackPosition)
                        {
                            nextVehicleEntity = segmentData.Entity;
                            nextVehicleBackPosition = segmentData.BackSegPosition;
                        }
                    }
                } while (vehicleSegmentMap.TryGetNextValue(out segmentData, ref nativeMultiHashMapIterator));
            }
        }

        public bool HasVehicleInSegment(
            NativeMultiHashMap<Entity, VehicleSegmentData> vehicleSegmentMap,
            Entity segmentEntity
        )
        {
            return vehicleSegmentMap.CountValuesForKey(segmentEntity) > 0;
        }
    }
}