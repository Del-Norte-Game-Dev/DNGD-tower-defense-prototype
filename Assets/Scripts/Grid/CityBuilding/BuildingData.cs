using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingData", menuName = "BuildingData")]
public class BuildingData : ScriptableObject
{
    public BuildingShapeType type = BuildingShapeType.Rectangular;

    public GameObject prefab;

    [Header("Rectangular")]
    public Vector2Int size;
    private Vector2Int _lastSize;

    [Header("Irregular")]
    public List<Vector2Int> footprint;

    private void OnValidate()
    {
        if (type == BuildingShapeType.Rectangular && size != _lastSize)
        {
            _lastSize = size;
            GenerateRectangularFootprint();
        }
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
}
