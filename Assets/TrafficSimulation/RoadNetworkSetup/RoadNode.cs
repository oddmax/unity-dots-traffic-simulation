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
            Mesh coneMesh = null;
            if (transform.parent.GetComponent<RoadSetup>() != null)
                coneMesh = transform.parent.GetComponent<RoadSetup>().ConeMesh;
            
            if (transform.parent.GetComponent<RoadPiece>() != null)
                coneMesh = transform.parent.GetComponent<RoadPiece>().ConeMesh;
            
            if(coneMesh != null)
                Gizmos.DrawMesh(coneMesh, transform.position, transform.rotation,
                size);
        }
    #endif
    }
}