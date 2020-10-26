using Unity.Entities;

namespace TrafficSimulation.Components.General
{
    public struct TimerComponent : IComponentData
    {
        public float TimeInSeconds;
        public float TimeLeft;
        public bool IsTimerOver;

        public static TimerComponent CreateTimer(float time)
        {
            return new TimerComponent
            {
                TimeInSeconds = time,
                TimeLeft = time,
                IsTimerOver = false
            };
        }
    }
}