using TrafficSimulation.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace TrafficSimulation.Systems
{
    //[UpdateBefore(typeof(VehicleMovementSystem))]
    //[UpdateInGroup(typeof(PresentationSystemGroup))]
    /*public class TrafficVehicleSegmentChooseSystem : JobComponentSystem
    {
        EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
        private EntityQuery group;
        private EntityQuery segmentsEntityQuery;

        protected override void OnCreate()
        {
            base.OnCreate();
            endSimulationEcbSystem = World
                .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            @group = GetEntityQuery(typeof(VehicleComponent), ComponentType.ReadOnly<HasReachedNodeComponent>());

            segmentsEntityQuery = GetEntityQuery(typeof(SegmentComponent));
        }
        
        [BurstCompile]
        private struct ChooseNextSegmentJob : IJobChunk
        {
            public ArchetypeChunkComponentType<VehicleComponent> vehicleType;
            [ReadOnly] public ArchetypeChunkComponentType<HasReachedNodeComponent> hasReachedNodeType;
            public EntityCommandBuffer.Concurrent endSimulationEcbSystem { get; set; }
            
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<SegmentComponent> segmentComponentArray;
            [ReadOnly] public ComponentDataFromEntity<SegmentComponent> segmentComponentData;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                var chunkVehicleComponent = chunk.GetNativeArray(vehicleType);
                for (var i = 0; i < chunk.Count; i++)
                {
                    var segment = segmentComponentData[chunkVehicleComponent[i].Segment];
                    var node = segment.EndNode;
                    for (var j = 0; j < segmentComponentArray.Length; j++)
                    {
                        if (node == segmentComponentArray[j].StartNode)
                        {
                            
                        }
                    }
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var ecb = endSimulationEcbSystem.CreateCommandBuffer().ToConcurrent();

            var segmentComponentData = GetComponentDataFromEntity<SegmentComponent>();
            NativeArray<SegmentComponent> segmentComponentArray = segmentsEntityQuery.ToComponentDataArray<SegmentComponent>(Allocator.TempJob);
            
            var vehicleType = GetArchetypeChunkComponentType<VehicleComponent>();
            var hasReachedNodeType = GetArchetypeChunkComponentType<HasReachedNodeComponent>(true);
            
            var job = new ChooseNextSegmentJob()
            {
                endSimulationEcbSystem = ecb,
                vehicleType = vehicleType,
                hasReachedNodeType = hasReachedNodeType,
                segmentComponentArray = segmentComponentArray,
                segmentComponentData = segmentComponentData
            }.Schedule(@group);

            endSimulationEcbSystem.AddJobHandleForProducer(job);
            
            return job;
        }

       
       
    }*/
}