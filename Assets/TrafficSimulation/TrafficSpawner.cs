using System;
using System.Collections.Generic;
using TrafficSimulation;
using TrafficSimulation.Components;
using TrafficSimulation.Components.Buffers;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrafficSpawner : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public int CarToSpawn = 5;
    public GameObject CarPrefab;
    public List<RoadNode> NodesToCreate;
    public List<RoadSegment> SegmentsToCreate;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            Entity carPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(CarPrefab,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));

            Dictionary<RoadNode, Entity> roadNodes = new Dictionary<RoadNode, Entity>();

            foreach (var roadNode in NodesToCreate)
            {
                var roadNodeEntity = dstManager.CreateEntity(typeof(RoadNodeComponent));
                dstManager.AddBuffer<ConnectedSegmentBufferElement>(roadNodeEntity);
                var roadNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(roadNodeEntity);
                roadNodeComponent.Position = roadNode.transform.position;
                
                dstManager.SetComponentData(roadNodeEntity, roadNodeComponent);
                
                roadNodes.Add(roadNode, roadNodeEntity);
            }
            
            List<Entity> segments = new List<Entity>();
            foreach (var segment in SegmentsToCreate)
            {
                var segmentEntity = dstManager.CreateEntity(typeof(SegmentComponent));
                
                var segmentComponent = dstManager.GetComponentData<SegmentComponent>(segmentEntity);
                segmentComponent.StartNode = roadNodes[segment.StartNode];
                segmentComponent.EndNode = roadNodes[segment.EndNode];
                segmentComponent.Length = Vector3.Distance(segment.StartNode.transform.position,
                    segment.EndNode.transform.position);
                segmentComponent.MovementDirection = segment.EndNode.transform.position -
                                                     segment.StartNode.transform.position;

                var nodeBuffer = dstManager.GetBuffer<ConnectedSegmentBufferElement>(segmentComponent.StartNode);
                nodeBuffer.Add(new ConnectedSegmentBufferElement { segment = segmentEntity });
                
                dstManager.SetComponentData(segmentEntity, segmentComponent);
                
                segments.Add(segmentEntity);
            }

            for (int i = 0; i < CarToSpawn; i++)
            {
                var carEntity = dstManager.Instantiate(carPrefab);

                var vehicleComponent = dstManager.GetComponentData<VehicleComponent>(carEntity);

                var segmentIndex = Random.Range(0, segments.Count);
                var segmentEntity = segments[segmentIndex];
                var segmentComponent = dstManager.GetComponentData<SegmentComponent>(segmentEntity);

                vehicleComponent.Segment = segmentEntity;
                vehicleComponent.CurrentSegPos = Random.Range(0f, 1f);
                
                
                var translation = dstManager.GetComponentData<Translation>(carEntity);
                
                var startNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(segmentComponent.StartNode);
                var endNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(segmentComponent.EndNode);
                
                translation.Value = math.lerp(endNodeComponent.Position, startNodeComponent.Position, vehicleComponent.CurrentSegPos);
                vehicleComponent.Target = endNodeComponent.Position;
                
                dstManager.SetComponentData(carEntity, vehicleComponent);
                dstManager.SetComponentData(carEntity, translation);
            }
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(CarPrefab);
    }
}
