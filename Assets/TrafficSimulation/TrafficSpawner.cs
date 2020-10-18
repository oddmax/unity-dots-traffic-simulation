using System;
using System.Collections.Generic;
using TrafficSimulation;
using TrafficSimulation.Components.Buffers;
using TrafficSimulation.Components.Road;
using TrafficSimulation.Components.Vehicle;
using TrafficSimulation.RoadNetworkSetup;
using TrafficSimulation.Systems;
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

    private EntityManager dstManager;
    private List<Entity> roadSegments;
    private Entity carEntityBase;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        roadNetworkGenerator = new RoadNetworkGenerator(dstManager);
        using (BlobAssetStore blobAssetStore = new BlobAssetStore())
        {
            this.carEntityBase =  GameObjectConversionUtility.ConvertGameObjectHierarchy(CarPrefab,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            
            roadNetworkGenerator.GenerateNetwork(RoadPieces, out var roadNodes, out var roadSegments);

            this.roadSegments = roadSegments;
            this.dstManager = dstManager;
            SpawnCars(dstManager, carEntityBase, roadSegments);
        }
    }

    private void SpawnCars(EntityManager dstManager, Entity carEntity, List<Entity> roadSegments)
    {
        for (int i = 0; i < CarToSpawn; i++)
        {
            SpawnCar();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            SpawnCar();
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(CarPrefab);
    }

    private void SpawnCar()
    {
        var carEntity = dstManager.Instantiate(carEntityBase);
        
        var vehicleComponent = dstManager.GetComponentData<VehiclePositionComponent>(carEntity);
        var vehicleSegmentInfoComponent = dstManager.GetComponentData<VehicleSegmentInfoComponent>(carEntity);
        var vehicleMoveIntentionComponent = dstManager.GetComponentData<VehicleSegmentChangeIntention>(carEntity);
        var vehicleConfig = dstManager.GetComponentData<VehicleConfigComponent>(carEntity);

        var segmentEntity = GetRandomSegmentWithFreeSpace(vehicleConfig.Length/2, vehicleConfig.Length);
        var segmentComponent = dstManager.GetComponentData<SegmentConfigComponent>(segmentEntity);
        var splineComponent = dstManager.GetComponentData<SplineComponent>(segmentEntity);

        vehicleComponent.HeadSegPos = vehicleConfig.Length;
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
        
        var centerVehicleSegPos = vehicleComponent.HeadSegPos - vehicleConfig.Length / 2;
        var currentPos = splineComponent.Point(centerVehicleSegPos / splineComponent.Length);
        var nextPos = splineComponent.Point(centerVehicleSegPos + 0.1f / splineComponent.Length);
        var directionVector = nextPos - currentPos;

        dstManager.SetComponentData(carEntity, new Translation{Value = splineComponent.Point(centerVehicleSegPos / splineComponent.Length)});
        dstManager.SetComponentData(carEntity, new Rotation {Value = quaternion.LookRotation(directionVector, math.up())});
        dstManager.SetComponentData(carEntity, vehicleComponent);
        dstManager.SetComponentData(carEntity, vehicleSegmentInfoComponent);
        dstManager.SetComponentData(carEntity, vehicleMoveIntentionComponent);
    }

    private Entity GetRandomSegmentWithFreeSpace(float position, float size)
    {
        var vehiclesSegmentsHashMap = CalculateCarsInSegmentsSystem.VehiclesSegmentsHashMap;
        var helper = new VehiclesInSegmentHashMapHelper();

        for (int i = 0; i < 10; i++)
        {
            var segmentIndex = Random.Range(0, roadSegments.Count);
            var segmentEntity = roadSegments[segmentIndex];
            if (helper.IsSpaceAvailableAt(vehiclesSegmentsHashMap, segmentEntity, position, size))
                return segmentEntity;
        }
        
        return Entity.Null;;
    }
}
