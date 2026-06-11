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
    }

    private void OnDisable()
    {
        if (buildManager != null)
        {
            buildManager.BuildingPlaced -= HandleBuildingPlaced;
            buildManager.BuildingRemoved -= HandleBuildingRemoved;
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

    private void HandleBuildingPlaced(List<Vector2Int> cells)
    {
        if (mapProvider == null)
            return;

        foreach (Vector2Int cell in cells)
        {
            mapProvider.SetWalkable(cell.x, cell.y, false);
        }
    }

    private void HandleBuildingRemoved(List<Vector2Int> cells)
    {
        if (mapProvider == null)
            return;

        foreach (Vector2Int cell in cells)
        {
            mapProvider.SetWalkable(cell.x, cell.y, true);
        }
    }
}
