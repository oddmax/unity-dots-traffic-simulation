using Unity.Mathematics;
using UnityEngine;

namespace TrafficSimulation
{
    public class RoadSegment : MonoBehaviour
    {
        public RoadNode StartNode;
        public RoadNode EndNode;
        public float3 MovementDirection;
        
        [HideInInspector]
        public float Length;
    }
}