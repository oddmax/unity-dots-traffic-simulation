using System;
using UnityEngine;

namespace TrafficSimulation.RoadNetworkSetup
{
    [Serializable]
    public struct RoadIntersectionSegmentsGroup
    {
        public RoadSegment[] Segments;
        public int Time;
    }
    
    public class RoadIntersection : MonoBehaviour
    {
        [SerializeField]
        private RoadIntersectionSegmentsGroup[] intersectionGroups;
    }
}