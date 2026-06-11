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
        InitializePlainCosts();

        config = GlobalAssets.FlowFieldVisual;
        debugDrawer = new GameObject("MapDebugDrawer").AddComponent<GridDebugDrawer>();
        debugDrawer.transform.SetParent(transform);
        debugDrawer.Initialize(grid);

        debugDrawer.SetColor(true,
            (x, y) =>
            {
                grid.TryGetGridObject(x, y, out MapCell cell);
                if (cell == null || !cell.IsWalkable())
                    return config.impassibleColor;

                float t = cell.cost / 5f;
                return Color.Lerp(config.minCostColor, config.maxCostColor, Mathf.Clamp01(t));
            });
        // debugDrawer.SetText(true);
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
                float noise = Mathf.PerlinNoise((x + seedX) * noiseScale, (y + seedY) * noiseScale); // can return value outside of [0f, 1f]
                noise = Mathf.Clamp01(noise);
                
                MapCell cell = grid.GetGridObject(x, y);
                if (cell == null) continue;

                byte cost = noise > obstacleThreshold ? byte.MaxValue : (byte)(1 + Mathf.FloorToInt(noise * 5));
                cell.SetCost(cost);
            }
        }
    }
    # endregion

    public bool SetCost(int x, int y, byte cost)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return false;
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
        return cell.SetCost(cost);
    }

    public bool IncreaseCost(int x, int y, byte cost)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return false;
        bool r =cell.IncreaseCost(cost);
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
        return r;
    }

    public bool DecreaseCost(int x, int y, byte cost)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return false;
        bool r = cell.DecreaseCost(cost);
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
        return r;
    }

    public void SetWalkable(int x, int y, bool isWalkable)
    {
        if(!grid.TryGetGridObject(x, y, out MapCell cell)) return;
        cell.SetWalkable(isWalkable);
        grid.TriggerDebugRefresh(x, y);
        CostGridChanged?.Invoke();
    }
}