using System.Collections.Generic;
using UnityEngine;

public class GameGridManager : GenericSingleton<GameGridManager>
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 100;
    [SerializeField] private int height = 100;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;
    [SerializeField] private Vector3 defaultDestination = new Vector3(50f, 50f, 0f);

    [Header("Dependencies")]
    [SerializeField] private BuildingRegistry buildingRegistry;
    [SerializeField] private string worldSaveFileName = "world_save.json";

    private MapProvider mapProvider;
    private FlowFieldManager flowFieldManager;
    private BuildManager buildManager;
    private EnemyWaveManager enemyWaveManager;

    protected override void Awake()
    {
        base.Awake();

        mapProvider = CreateOrFindManager<MapProvider>("MapProvider");
        flowFieldManager = CreateOrFindManager<FlowFieldManager>("FlowFieldManager");
        buildManager = CreateOrFindManager<BuildManager>("BuildManager");
        enemyWaveManager = CreateOrFindManager<EnemyWaveManager>("EnemyWaveManager");

        InitializeManagers();
        SubscribeEvents();
        LoadSavedBuildings();
    }

    private void OnDisable()
    {
        if (buildManager != null)
        {
            buildManager.BuildingPlaced -= HandleBuildingPlaced;
            buildManager.BuildingRemoved -= HandleBuildingRemoved;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveWorld();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            LoadSavedBuildings();
        }
    }

    private T CreateOrFindManager<T>(string name) where T : GenericSingleton<T>
    {
        T instance = FindAnyObjectByType<T>();
        if (instance != null)
            return instance;

        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go.AddComponent<T>();
    }

    private void InitializeManagers()
    {
        mapProvider.Initialize(width, height, cellSize, origin);
        flowFieldManager.Initialize(mapProvider, defaultDestination);
        buildManager.Initialize(buildingRegistry, width, height, cellSize, origin);
        enemyWaveManager.Initialize(width, height, cellSize, defaultDestination);
    }

    private void SubscribeEvents()
    {
        buildManager.BuildingPlaced += HandleBuildingPlaced;
        buildManager.BuildingRemoved += HandleBuildingRemoved;
    }

    private void HandleBuildingPlaced(PlacedBuilding placed)
    {
        if (mapProvider == null || placed == null)
            return;

        foreach (Vector2Int cell in placed.OccupiedPositions)
        {
            mapProvider.IncreaseCost(cell.x, cell.y, 100);
        }

        foreach (Vector2Int cell in placed.OccupiedCostPositions){
            mapProvider.IncreaseCost(cell.x, cell.y, (byte)placed.Data.costIncrement);
        }
    }

    private void HandleBuildingRemoved(PlacedBuilding placed)
    {
        if (mapProvider == null || placed == null)
            return;

        foreach (Vector2Int cell in placed.OccupiedPositions)
        {
            mapProvider.DecreaseCost(cell.x, cell.y, 100);
        }

        foreach (Vector2Int cell in placed.OccupiedCostPositions){
            mapProvider.DecreaseCost(cell.x, cell.y, (byte)placed.Data.costIncrement);
        }
    }

    private void LoadSavedBuildings()
    {
        if (string.IsNullOrEmpty(worldSaveFileName))
            return;

        WorldSaveData saveData = DataLoader.LoadJson<WorldSaveData>(worldSaveFileName);
        if (saveData == null || saveData.buildings == null || saveData.buildings.Count == 0)
            return;

        foreach (BuildingSaveData entry in saveData.buildings)
        {
            if (string.IsNullOrEmpty(entry.buildingID))
                continue;

            BuildingData data = buildingRegistry?.Get(entry.buildingID);
            if (data == null)
            {
                Debug.LogWarning($"Saved building not found in registry: {entry.buildingID}");
                continue;
            }

            BuildingData.Dir dir = BuildingData.GetDirFromRot(entry.rotation);
            if (!buildManager.PlaceBuildingAtOrigin(data, dir, entry.origin))
            {
                Debug.LogWarning($"Failed to place saved building {entry.buildingID} at {entry.origin} rotation {entry.rotation}");
            }
        }
    }

    [ContextMenu("Save World")]
    private void SaveWorld()
    {
        if (buildManager == null)
        {
            Debug.LogWarning("BuildManager is not initialized. Cannot save world.");
            return;
        }

        WorldSaveData saveData = buildManager.ToWorldSaveData();
        if (saveData == null)
        {
            Debug.LogWarning("Nothing to save.");
            return;
        }

        DataLoader.SaveJson(worldSaveFileName, saveData);
    }
}
