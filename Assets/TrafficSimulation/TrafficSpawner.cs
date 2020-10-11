using System;
using System.Collections.Generic;
using TrafficSimulation;
using TrafficSimulation.Components;
using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using TrafficSimulation.RoadNetworkSetup;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

            var segmentIndex = Random.Range(0, roadSegments.Count);
            var segmentEntity = roadSegments[segmentIndex];
            var segmentComponent = dstManager.GetComponentData<SegmentConfigComponent>(segmentEntity);

            vehicleComponent.HeadSegPos = segmentComponent.Length;
            vehicleComponent.BackSegPos = vehicleComponent.HeadSegPos - 0.2f;

            var translation = dstManager.GetComponentData<Translation>(carEntity);

            var startNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(segmentComponent.StartNode);
            var endNodeComponent = dstManager.GetComponentData<RoadNodeComponent>(segmentComponent.EndNode);

            vehicleSegmentInfoComponent.HeadSegment = segmentEntity;
            vehicleSegmentInfoComponent.BackSegment = segmentEntity;
            vehicleSegmentInfoComponent.SegmentLength = segmentComponent.Length;

            //var splineComponent = dstManager.GetComponentData<SplineComponent>(segmentEntity);

            //translation.Value = splineComponent.Point(vehicleComponent.HeadSegPos/segmentComponent.Length);

            dstManager.SetComponentData(carEntity, vehicleComponent);
            dstManager.SetComponentData(carEntity, vehicleSegmentInfoComponent);
            //dstManager.SetComponentData(carEntity, translation);
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(CarPrefab);
    }
}
