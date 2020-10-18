using Unity.Collections;
using Unity.Entities;

namespace TrafficSimulation.Systems
{
    /// <summary>
    /// Helper class to simplify readings from NativeHashMap which includes which vehicles are currently in which segment
    /// </summary>
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
        
        public bool IsSpaceAvailableAt(
            NativeMultiHashMap<Entity, VehicleSegmentData> vehicleSegmentMap,
            Entity segmentEntity,
            float position,
            float vehicleSize
        )
        {
            NativeMultiHashMapIterator<Entity> nativeMultiHashMapIterator;
            var vehicleFrontPos = position + vehicleSize / 2;
            var vehicleBackPos = vehicleFrontPos - vehicleSize;
            var canFit = true;
            if (vehicleSegmentMap.TryGetFirstValue(segmentEntity, out var segmentData, out nativeMultiHashMapIterator))
            {
                do
                {
                    if (vehicleFrontPos < segmentData.BackSegPosition)
                        continue;
                    
                    if(vehicleBackPos > segmentData.BackSegPosition + segmentData.VehicleSize)
                        continue;

                    canFit = false;

                } while (vehicleSegmentMap.TryGetNextValue(out segmentData, ref nativeMultiHashMapIterator));
            }

            return canFit;
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