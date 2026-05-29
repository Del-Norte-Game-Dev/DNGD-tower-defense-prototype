using UnityEngine;
using System.Collections.Generic;

public class PlacedBuilding
{
    public BuildingData Data { get; private set; }
    public Transform Transform { get; private set; }
    public List<BuildCell> OccupiedCells { get; private set; }

    public PlacedBuilding(BuildingData data, Transform transform, List<BuildCell> cells)
    {
        Data = data;
        Transform = transform;
        OccupiedCells = cells;
    }

    public void Remove()
    {
        foreach (var cell in OccupiedCells)
        {
            cell.ClearBuilding();
        }

        Object.Destroy(Transform.gameObject);
    }
}