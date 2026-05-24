using UnityEngine;

public class MapProvider : GenericSingleton<MapProvider>
{
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;
    private FlowFieldVisualConfig config;
    private GridDebugDrawer debugDrawer;

    private Grid<MapCell> grid;

    override protected void Awake()
    {
        base.Awake();
        grid = new Grid<MapCell>(
            width,
            height,
            cellSize,
            origin,
            (g, x, y) => new MapCell(x, y)
        );
        InitializeNoiseCosts();

        config = GlobalAssets.FlowFieldVisual;
        debugDrawer = new GameObject("DebugDrawer").AddComponent<GridDebugDrawer>();
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
                grid.GetGridObject(x, y).cost = 1;
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
        seedX = Random.Range(0f, 10000f);
        seedY = Random.Range(0f, 10000f);
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                float noise = Mathf.PerlinNoise((x + seedX) * noiseScale, (y + seedY) * noiseScale);

                MapCell cell = grid.GetGridObject(x, y);

                if (noise > obstacleThreshold)
                {
                    cell.cost = byte.MaxValue;
                }
                else
                {
                    cell.cost = (byte)(1 + Mathf.FloorToInt(noise * 5));
                }
            }
        }
    }
    # endregion

    public void SetCost(int x, int y, byte cost)
    {
        var cell = grid.GetGridObject(x, y);
        cell.cost = cost;
    }

    public void SetBlocked(int x, int y)
    {
        SetCost(x, y, byte.MaxValue);
    }

    public void SetWalkable(int x, int y, byte cost = 1)
    {
        SetCost(x, y, cost);
    }
}