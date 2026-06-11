using UnityEngine;
using System.Collections.Generic;

public class PlacedBuilding
{
    public BuildingData Data { get; private set; }
    public Transform Transform { get; private set; }
    public IReadOnlyList<Vector2Int> OccupiedPositions { get; private set; }
    public IReadOnlyList<Vector2Int> OccupiedCostPositions { get; private set; }

    private BuildMap map;

    public PlacedBuilding(
        BuildingData data, 
        Transform transform, 
        List<Vector2Int> positions,
        List<Vector2Int> costPositions,
        BuildMap map)
    {
        Data = data;
        Transform = transform;
        OccupiedPositions = positions.AsReadOnly();
        OccupiedCostPositions = costPositions.AsReadOnly();
        this.map = map;
    }

    public void Remove()
    {
        if (map != null)
        {
            List<Vector2Int> removed = new List<Vector2Int>();
            map.ClearBuildingCells(this, removed);
        }

        Object.Destroy(Transform.gameObject);
    }
}