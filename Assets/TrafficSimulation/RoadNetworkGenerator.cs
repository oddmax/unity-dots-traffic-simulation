using System.Collections.Generic;
using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.RoadNetworkSetup;
using Unity.Entities;
using UnityEngine;

namespace TrafficSimulation
{
    public class NodeGenerationInfo
    {
        public List<RoadNode> nodesAtSamePosition;
        public Vector3 position;
    }
    
    public class RoadNetworkGenerator
    {
        private const float MINIMUM_PROXIMITY = 0.01f; 
        private RoadPiece[] roadPieces;
        
        private List<NodeGenerationInfo> nodesList = new List<NodeGenerationInfo>();
        private Dictionary<RoadNode, NodeGenerationInfo> nodesMap = new Dictionary<RoadNode, NodeGenerationInfo>();
        private EntityManager dstManager;

        public RoadNetworkGenerator(EntityManager dstManager)
        {
            this.dstManager = dstManager;
        }

        public void GenerateNetwork(RoadPiece[] roadPieces, out List<Entity> roadNodes, out List<Entity> roadSegments)
        {
            this.roadPieces = roadPieces;
            
            FindNodesAtSamePositions();
            
            roadNodes = GenerateNodesEntities(out var roadNodesMap);
            roadSegments = GenerateSegmentEntities(roadNodesMap);
        }

        private List<Entity> GenerateSegmentEntities(Dictionary<RoadNode, Entity> roadNodes)
        {
            List<Entity> segments = new List<Entity>();
            foreach (var roadPiece in roadPieces)
            {
                foreach (var segment in roadPiece.RoadSegments)
                {
                    var segmentEntity = dstManager.CreateEntity(typeof(SegmentConfigComponent), typeof(SplineComponent),
                        typeof(SegmentComponent));

                    var splineComponent = SplineComponent.CreateSpline(segment.StartNode.transform, segment.EndNode.transform,
                        segment.CurveIn);

                    var segmentComponent = dstManager.GetComponentData<SegmentConfigComponent>(segmentEntity);
                    segmentComponent.StartNode = roadNodes[segment.StartNode];
                    segmentComponent.EndNode = roadNodes[segment.EndNode];
                    segmentComponent.Length = splineComponent.TotalLength();

                    var nodeBuffer = dstManager.GetBuffer<ConnectedSegmentBufferElement>(segmentComponent.StartNode);
                    nodeBuffer.Add(new ConnectedSegmentBufferElement {segment = segmentEntity});

                    dstManager.SetComponentData(segmentEntity, segmentComponent);
                    dstManager.SetComponentData(segmentEntity, splineComponent);
                    dstManager.SetComponentData(segmentEntity,
                        new SegmentComponent {AvailableLength = segmentComponent.Length});

                    segments.Add(segmentEntity);
                }
            }

            return segments;
        }

        private List<Entity> GenerateNodesEntities(out Dictionary<RoadNode, Entity> roadNodeMap)
        {
            roadNodeMap = new Dictionary<RoadNode, Entity>();
            var roadNodes = new List<Entity>();

            foreach (var roadNode in nodesList)
            {
                var roadNodeEntity = dstManager.CreateEntity(typeof(RoadNodeComponent));
                dstManager.AddBuffer<ConnectedSegmentBufferElement>(roadNodeEntity);
                var roadNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(roadNodeEntity);
                roadNodeComponent.Position = roadNode.position;

                dstManager.SetComponentData(roadNodeEntity, roadNodeComponent);
                
                roadNodes.Add(roadNodeEntity);

                foreach (var node in roadNode.nodesAtSamePosition)
                    roadNodeMap.Add(node, roadNodeEntity);
            }

            return roadNodes;
        }

        private void FindNodesAtSamePositions()
        {
            for (int i = 0; i < roadPieces.Length; i++)
            {
                var roadPiece = roadPieces[i];
                foreach (var roadNode in roadPiece.RoadNodes)
                    GenerateNodeInfo(roadNode, i);
            }
        }

        private NodeGenerationInfo GenerateNodeInfo(RoadNode checkedRoadNode, int index)
        {
            if (nodesMap.ContainsKey(checkedRoadNode))
                return null;
            
            var nodeInfo = new NodeGenerationInfo();
            nodeInfo.position = checkedRoadNode.transform.position;
            nodesMap.Add(checkedRoadNode, nodeInfo);
            nodesList.Add(nodeInfo);
            nodeInfo.nodesAtSamePosition.Add(checkedRoadNode);
                
            for (int i = index + 1; i < roadPieces.Length; i++)
            {
                var roadPiece = roadPieces[i];
                foreach (var roadNode in roadPiece.RoadNodes)
                {
                    if (Vector3.Distance(checkedRoadNode.transform.position, roadNode.transform.position) <
                        MINIMUM_PROXIMITY)
                    {
                        nodesMap.Add(roadNode, nodeInfo);
                        nodeInfo.nodesAtSamePosition.Add(roadNode);
                    }
                }
            }

            return nodeInfo;
        }

        
    }
}