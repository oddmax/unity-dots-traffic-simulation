using Model.Components;
using Unity.Mathematics;
using UnityEngine;

namespace TrafficSimulation.RoadNetworkSetup
{
    public class RoadSegment : MonoBehaviour
    {
        public float CurveIn = 1.0f;
        
        public RoadNode StartNode;
        public RoadNode EndNode;
        public float3 MovementDirection;
        private const float SegmentLength = 1f;
        public static readonly Vector3 OffsetZ = Vector3.up * 0.1f;
        
        [HideInInspector]
        public float Length;
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (StartNode != null && EndNode != null)
            {
                DrawConnection(DrawMode.Line | DrawMode.Arrow);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var mode = DrawMode.Selected | DrawMode.Line | DrawMode.Arrow;
            if (StartNode != null && EndNode != null)
            {
                DrawConnection(mode);
            }
        }

        private void DrawConnection(DrawMode mode)
        {
            var s = SplineComponent.CreateSpline(StartNode.transform, EndNode.transform, CurveIn);
            DrawSpline(mode, s);
        }
        
        private void DrawSpline(DrawMode mode, SplineComponent s)
        {
            int length = (int) (s.TotalLength() / SegmentLength);
            bool selected = (mode & DrawMode.Selected) != 0;
            if ((mode & DrawMode.Line) != 0)
            {
                for (int i = 0; i <= length - 1; i++)
                {
                    bool isEven = i % 2 == 0;
                    var startPoint = (Vector3) s.Point((float) i / length);
                    var endPoint = (Vector3) s.Point((float) (i + 1) / length);
                    var color = (selected
                            ? (isEven ? Color.red : Color.green)
                            : Color.white);
                    if ((mode & DrawMode.Darker) != 0) color *= 0.75f;
                    Gizmos.color = color;
                    Gizmos.DrawLine(startPoint + OffsetZ,
                        endPoint + OffsetZ);
                }
            }

            if ((mode & DrawMode.Arrow) != 0)
            {
                Gizmos.color = selected ? Color.cyan : Color.white;
                var center = s.Point(0.5f);
                var forward = s.Tangent(0.5f);
                Gizmos.DrawMesh(transform.parent.GetComponent<RoadSetup>().ConeMesh, center, Quaternion.LookRotation(forward),
                    new Vector3(1f, 1f, 2f));
            }
        }
            
        [System.Flags]
        private enum DrawMode
        {
            None = 0,
            Selected = 1 << 0,
            Line = 1 << 1,
            Arrow = 1 << 2,
            Darker = 1 << 4,
        }
#endif

    }
}