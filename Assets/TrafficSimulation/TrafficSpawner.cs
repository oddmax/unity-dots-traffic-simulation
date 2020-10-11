using System.Collections.Generic;
using TrafficSimulation;
using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using TrafficSimulation.RoadNetworkSetup;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrafficSpawner : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public int CarToSpawn = 5;
    public GameObject CarPrefab;
    public RoadPiece[] RoadPieces;

    private RoadNetworkGenerator roadNetworkGenerator;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        roadNetworkGenerator = new RoadNetworkGenerator(dstManager);
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            Entity carPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(CarPrefab,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            
            roadNetworkGenerator.GenerateNetwork(RoadPieces, out var roadNodes, out var roadSegments);

            SpawnCars(dstManager, carPrefab, roadSegments);
        }
    }

    private void SpawnCars(EntityManager dstManager, Entity carPrefab, List<Entity> roadSegments)
    {
        for (int i = 0; i < CarToSpawn; i++)
        {
            var carEntity = dstManager.Instantiate(carPrefab);

            var vehicleComponent = dstManager.GetComponentData<VehiclePositionComponent>(carEntity);
            var vehicleSegmentInfoComponent = dstManager.GetComponentData<VehicleSegmentInfoComponent>(carEntity);
            var vehicleMoveIntentionComponent = dstManager.GetComponentData<VehicleSegmentChangeIntention>(carEntity);
            var vehicleConfig = dstManager.GetComponentData<VehicleConfigComponent>(carEntity);

            var segmentIndex = Random.Range(0, roadSegments.Count);
            var segmentEntity = roadSegments[segmentIndex];
            var segmentComponent = dstManager.GetComponentData<SegmentConfigComponent>(segmentEntity);

            vehicleComponent.HeadSegPos = vehicleConfig.Length + 0.2f;
            vehicleComponent.BackSegPos = vehicleComponent.HeadSegPos - vehicleConfig.Length;

            vehicleSegmentInfoComponent.HeadSegment = segmentEntity;
            vehicleSegmentInfoComponent.IsBackInPreviousSegment = false;
            vehicleSegmentInfoComponent.PreviousSegment = Entity.Null;
            vehicleSegmentInfoComponent.SegmentLength = segmentComponent.Length;
            vehicleSegmentInfoComponent.NextNode = segmentComponent.EndNode;
            
            var nodeBuffer = dstManager.GetBuffer<ConnectedSegmentBufferElement>(segmentComponent.EndNode);
            if (nodeBuffer.Length > 0)
            {
                var randomNextSegment = Random.Range(0, nodeBuffer.Length);
                vehicleMoveIntentionComponent.NextSegment = nodeBuffer[randomNextSegment].segment;
            }
            
            dstManager.SetComponentData(carEntity, vehicleComponent);
            dstManager.SetComponentData(carEntity, vehicleSegmentInfoComponent);
            dstManager.SetComponentData(carEntity, vehicleMoveIntentionComponent);
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(CarPrefab);
    }
}
