using Unity.Collections;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;

namespace TrafficSimulation.Systems
{
    /// <summary>
    /// Provides random number generation for usage in Unity ECS
    /// https://reeseschultz.com/random-number-generation-with-unity-dots/
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    class RandomSystem : ComponentSystem
    {
        public NativeArray<Unity.Mathematics.Random> RandomArray { get; private set; }

        protected override void OnCreate()
        {
            var randomArray = new Unity.Mathematics.Random[JobsUtility.MaxJobThreadCount];
            var seed = new System.Random();

            for (int i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
                randomArray[i] = new Unity.Mathematics.Random((uint)seed.Next());

            RandomArray = new NativeArray<Unity.Mathematics.Random>(randomArray, Allocator.Persistent);
        }

        protected override void OnDestroy()
            => RandomArray.Dispose();

        protected override void OnUpdate() { }
    }
}