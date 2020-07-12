using UnityEngine;

namespace TrafficSimulation.RoadNetworkSetup
{
    public class RoadNode : MonoBehaviour
    {
#if UNITY_EDITOR
        private readonly Vector3 size = new Vector3(1f, 1f, 2f);

        private void OnDrawGizmos()
        {

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, 0.5f);
            Gizmos.DrawMesh(transform.parent.GetComponent<RoadSetup>().ConeMesh, transform.position, transform.rotation,
                size);
        }
    #endif
    }
}