using System;
using UnityEngine;
using System.Collections.Generic;

public class Grid<TGridObject> : IGridDebug
{
    public event Action<int, int> OnGridObjectChanged;
    public event Action OnGridFullRefresh;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public Grid(
        int width,
        int height,
        float cellSize,
        Vector3 originPosition,
        Func<Grid<TGridObject>, int, int, TGridObject> createGridObject) //injected constructor
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                gridArray[x, y] = createGridObject(this, x, y);
            }
        }
    }

    public int GetWidth() => width;
    public int GetHeight() => height;
    public float GetCellSize() => cellSize;
    public Vector3 GetOriginPosition() => originPosition;

    public Vector3 GetWorldPositionCorner(int x, int y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public Vector3 GetWorldPositionCenter(int x, int y)
    {
        return new Vector3(x + 0.5f, y + 0.5f) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
    }

    public void SetGridObject(int x, int y, TGridObject value)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            gridArray[x, y] = value;
            OnGridObjectChanged?.Invoke(x, y);
        }
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXY(worldPosition, out int x, out int y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height) return gridArray[x, y];
        return default;
    }

    public bool TryGetGridObject(int x, int y, out TGridObject obj)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            obj = gridArray[x, y];
            return true;
        }

        obj = default;
        return false;
    }

    public bool TryGetGridObject(Vector3 worldPosition, out TGridObject obj)
    {
        GetXY(worldPosition, out int x, out int y);
        return TryGetGridObject(x, y, out obj);
    }

    public void TriggerDebugRefresh(int x, int y)
    {
        OnGridObjectChanged?.Invoke(x, y);
    }

    public void TriggerDebugFullRefresh()
    {
        OnGridFullRefresh?.Invoke();   
    }

    public List<TGridObject> GetNeighbors(int x, int y, IEnumerable<Vector2Int> directions)
    {
        List<TGridObject> neighbors = new List<TGridObject>();
        foreach(Vector2Int direction in directions)
        {
            if ( TryGetGridObject(x + direction.x, y + direction.y, out TGridObject neighbor))
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    #region static neighbor lists
    public static readonly Vector2Int[] DIR8_Default = {
        new Vector2Int(-1,  1), // up-left
        new Vector2Int( 0,  1), // up
        new Vector2Int( 1,  1), // up-right
        new Vector2Int(-1,  0), // left
        new Vector2Int( 1,  0), // right
        new Vector2Int(-1, -1), // down-left
        new Vector2Int( 0, -1), // down
        new Vector2Int( 1, -1), // down-right
    };

    public static readonly Vector2Int[] DIR8_ManhattanBias = {
        new Vector2Int( 0,  1), // up
        new Vector2Int(-1,  0), // left
        new Vector2Int( 1,  0), // right
        new Vector2Int( 0, -1), // down
        new Vector2Int(-1,  1), // up-left
        new Vector2Int( 1,  1), // up-right
        new Vector2Int(-1, -1), // down-left
        new Vector2Int( 1, -1), // down-right
    };


    public static readonly Vector2Int[] DIR8_DiagonalBias = {
        new Vector2Int(-1,  1), // up-left
        new Vector2Int( 1,  1), // up-right
        new Vector2Int(-1, -1), // down-left
        new Vector2Int( 1, -1), // down-right
        new Vector2Int( 0,  1), // up
        new Vector2Int(-1,  0), // left
        new Vector2Int( 1,  0), // right
        new Vector2Int( 0, -1), // down
    };

    public static readonly Vector2Int[] DIR4 = {
        new Vector2Int( 0,  1), // up
        new Vector2Int(-1,  0), // left
        new Vector2Int( 1,  0), // right
        new Vector2Int( 0, -1), // down
    };
    #endregion
    
    public string GetDebugValue(int x, int y)
    {
        TryGetGridObject(x, y, out TGridObject obj);
        return obj?.ToString();
    }
}

public interface IGridDebug
{
    int GetWidth();
    int GetHeight();
    float GetCellSize();
    Vector3 GetWorldPositionCorner(int x, int y);
    public string GetDebugValue(int x, int y);
    public event Action<int, int> OnGridObjectChanged;
    public event Action OnGridFullRefresh;
}