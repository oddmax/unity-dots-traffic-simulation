using Unity.Entities;

namespace TrafficSimulation.Components.Road
{
    public struct AddBlockLengthComponent : IComponentData
    {
        public float blockedLength;
    }
}