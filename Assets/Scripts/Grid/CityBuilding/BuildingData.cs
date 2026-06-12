using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ResourceEntry
{
    public ResourceType type;
    public int amount;
}

[CreateAssetMenu(fileName = "BuildingData", menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    [SerializeField] private string id;
    public string ID => id;

    public BuildingShapeType type = BuildingShapeType.Rectangular;

    
    public List<ResourceEntry> cost;

    public GameObject prefab;

    [Header("Rectangular")]
    public Vector2Int size;
    private Vector2Int _lastSize;
    [Header("Irregular")]
    public List<Vector2Int> footprint;

    [Header("Area Cost")]
    public int costIncrement = 1;
    public int radius = 1; // square
    public List<Vector2Int> costFootprint;

    private void OnValidate()
    {
        if (type == BuildingShapeType.Rectangular && size != _lastSize)
        {
            _lastSize = size;
            GenerateRectangularFootprint();
        }

        if ((footprint == null || costIncrement == 0 || radius == 0) && costFootprint == null)
            return;
        GenerateCostFootprint();
    }

    private void GenerateRectangularFootprint()
    {
        footprint.Clear();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                footprint.Add(new Vector2Int(x, y));
            }
        }
    }

    private void GenerateCostFootprint()
    {
        costFootprint = new List<Vector2Int>();

        if (footprint == null || footprint.Count == 0)
            return;

        HashSet<Vector2Int> footprintSet = new HashSet<Vector2Int>(footprint);
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();

        foreach (var cell in footprint)
        {
            queue.Enqueue((cell, 0));
            visited.Add(cell);
        }

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
        };

        while (queue.Count > 0)
        {
            var (pos, dist) = queue.Dequeue();

            if (dist > radius)
                continue;

            // Don't include original footprint tiles
            if (!footprintSet.Contains(pos))
            {
                costFootprint.Add(pos);
            }

            if (dist == radius)
                continue;

            foreach (var dir in directions)
            {
                Vector2Int next = pos + dir;

                if (visited.Contains(next))
                    continue;

                visited.Add(next);
                queue.Enqueue((next, dist + 1));
            }
        }
    }

    public Vector2 GetCenterOffset()
    {
        if (footprint == null || footprint.Count == 0)
            return Vector2.zero;

        int minX = footprint[0].x;
        int maxX = footprint[0].x;
        int minY = footprint[0].y;
        int maxY = footprint[0].y;

        foreach (var pos in footprint)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        return new Vector2(
            (minX + maxX) / 2f,
            (minY + maxY) / 2f
        );
    }

    public static Vector2 GetRotatedVector(Dir dir, float x, float y)
    {
        switch (dir)
        {
            case Dir.Right: return new Vector2(x, y);
            case Dir.Down: return new Vector2(y, -x);
            case Dir.Left: return new Vector2(-x, -y);
            case Dir.Up: return new Vector2(-y, x);
            default: return new Vector2(x, y);
        }
    }

    public enum BuildingShapeType
    {
        Rectangular,
        Irregular
    }

    public enum Dir
    {
        Right,
        Down,
        Left,
        Up
    }

    public static Dir GetNextDir(Dir dir)
    {
        switch (dir)
        {
            case Dir.Right: return Dir.Down;
            case Dir.Down: return Dir.Left;
            case Dir.Left: return Dir.Up;
            case Dir.Up: return Dir.Right;
            default: return Dir.Right;
        }
    }

    public static float GetRotFromDir(Dir dir)
    {
        switch (dir)
        {
            case Dir.Right: return 0;
            case Dir.Down: return -90;
            case Dir.Left: return 180;
            case Dir.Up: return 90;
            default: return 0;
        }
    }

    public static Dir GetDirFromRot(int rotation)
    {
        if (rotation >= 0 && rotation <= 3)
            return (Dir)rotation;

        int normalized = ((rotation % 360) + 360) % 360;
        switch (normalized)
        {
            case 90: return BuildingData.Dir.Up;
            case 180: return BuildingData.Dir.Left;
            case 270: return BuildingData.Dir.Down;
            default: return BuildingData.Dir.Right;
        }
    }
}
