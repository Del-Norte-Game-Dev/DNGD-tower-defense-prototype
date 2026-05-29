using System;
using UnityEngine;

public class MapProvider : GenericSingleton<MapProvider>
{
    public event Action CostGridChanged;

    [SerializeField] private int width = 100;
    [SerializeField] private int height = 100;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;
    private FlowFieldVisualConfig config;
    private GridDebugDrawer debugDrawer;

    private Grid<MapCell> grid;
    private bool isInitialized;

    public void Initialize(int width, int height, float cellSize, Vector3 origin)
    {
        if (isInitialized)
            return;

        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.origin = origin;

        grid = new Grid<MapCell>(
            width,
            height,
            cellSize,
            origin,
            (g, x, y) => new MapCell(x, y)
        );
        InitializeNoiseCosts();

        config = GlobalAssets.FlowFieldVisual;
        debugDrawer = new GameObject("MapDebugDrawer").AddComponent<GridDebugDrawer>();
        debugDrawer.transform.SetParent(transform);
        debugDrawer.Initialize(grid);

        debugDrawer.SetColor(true,
            (x, y) =>
            {
                grid.TryGetGridObject(x, y, out MapCell cell);
                if (cell.cost == byte.MaxValue)
                    return config.impassibleColor;

                float t = cell.cost / 5f;
                return Color.Lerp(config.minCostColor, config.maxCostColor, Mathf.Clamp01(t));
            });

        isInitialized = true;
    }

    public Grid<MapCell> GetGrid()
    {
        return grid;
    }

    public MapCell GetCell(int x, int y)
    {
        return grid.GetGridObject(x, y);
    }

    public bool TryGetCell(Vector3 worldPos, out MapCell cell)
    {
        return grid.TryGetGridObject(worldPos, out cell);
    }

    private void InitializePlainCosts()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                grid.GetGridObject(x, y).SetBaseCost(1);
                grid.GetGridObject(x, y).SetCost(1);
            }
        }
    }

    #region temp procedural noise cost mamp
    [SerializeField] private float noiseScale = 0.1f;
    [SerializeField] private float obstacleThreshold = 0.8f;
    float seedX; 
    float seedY; 

    private void InitializeNoiseCosts()
    {
        seedX = UnityEngine.Random.Range(0f, 10000f);
        seedY = UnityEngine.Random.Range(0f, 10000f);
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                float noise = Mathf.PerlinNoise((x + seedX) * noiseScale, (y + seedY) * noiseScale);

                MapCell cell = grid.GetGridObject(x, y);
                if (cell == null) continue;

                byte cost = noise > obstacleThreshold ? byte.MaxValue : (byte)(1 + Mathf.FloorToInt(noise * 5));
                cell.SetBaseCost(cost);
                cell.SetCost(cost);
            }
        }
    }
    # endregion

    public void SetCost(int x, int y, byte cost)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return;
        cell.SetCost(cost);
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
    }

    public void SetBlocked(int x, int y)
    {
        SetCost(x, y, byte.MaxValue);
    }

    public void RestoreOriginalCost(int x, int y)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return;
        cell.ResetToOriginalCost();
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
    }
}