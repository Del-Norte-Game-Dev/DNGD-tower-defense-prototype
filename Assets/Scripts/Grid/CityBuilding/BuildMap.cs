using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class BuildMap
{
    public Grid<BuildCell> BuildGrid => buildGrid;
    private Grid<BuildCell> buildGrid;

    public BuildMap(int width, int height, float cellSize, Vector3 origin)
    {
        buildGrid = new Grid<BuildCell>(width, height, cellSize, origin, (g, x, y) => new BuildCell(g, x, y));
    }

    public bool TryGetPlacement(Vector3 worldPos, BuildingData data, BuildingData.Dir dir, out Vector3 pfPos)
    {
        pfPos = Vector3.zero;

        if (!TryGetPlacementCells(worldPos, data, dir, out Vector2Int origin, out _))
            return false;

        pfPos = GetPlacementWorldCorner(dir, origin);
        return true;
    }

    public bool TryGetPlacementCells(Vector3 worldPos, BuildingData data, BuildingData.Dir dir, out Vector3 pfPos, out List<BuildCell> cells)
    {
        pfPos = Vector3.zero;
        cells = null;

        if (!TryGetPlacementCells(worldPos, data, dir, out Vector2Int origin, out cells))
            return false;

        pfPos = GetPlacementWorldCorner(dir, origin);
        return true;
    }

    public bool PlaceBuilding(PlacedBuilding placedBuilding)
    {
        if (placedBuilding == null || placedBuilding.OccupiedPositions == null)
            return false;

        int placedCount = 0;
        foreach (Vector2Int pos in placedBuilding.OccupiedPositions)
        {
            BuildCell cell = buildGrid.GetGridObject(pos.x, pos.y);
            if (cell == null) continue;
            cell.SetBuilding(placedBuilding);
            placedCount++;
        }

        return placedCount > 0;
    }

    private bool TryGetPlacementCells(Vector3 worldPos, BuildingData data, BuildingData.Dir dir, out Vector2Int origin, out List<BuildCell> validCells)
    {
        origin = GetPlacementOrigin(worldPos, data, dir);
        validCells = new List<BuildCell>(data.footprint?.Count ?? 0);

        foreach (Vector2Int offset in GetRotatedFootprint(data.footprint, dir))
        {
            int cellX = origin.x + offset.x;
            int cellY = origin.y + offset.y;

            if (!buildGrid.TryGetGridObject(cellX, cellY, out BuildCell cell) || !cell.CanBuild())
            {
                validCells = null;
                return false;
            }

            validCells.Add(cell);
        }

        return true;
    }

    public Vector2Int GetPlacementOrigin(Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        Vector2 centerOffset = data.GetCenterOffset();
        Vector2 originWorldPosition = (Vector2)worldPos - BuildingData.GetRotatedVector(dir, centerOffset.x, centerOffset.y);
        buildGrid.GetXY(originWorldPosition, out int x, out int y);
        return new Vector2Int(x, y);
    }

    public static List<Vector2Int> GetRotatedFootprint(List<Vector2Int> footprint, BuildingData.Dir dir)
    {
        List<Vector2Int> rotated = new List<Vector2Int>(footprint?.Count ?? 0);

        if (footprint == null)
            return rotated;

        foreach (Vector2Int pos in footprint)
        {
            Vector2 rotatedVec = BuildingData.GetRotatedVector(dir, pos.x, pos.y);
            rotated.Add(new Vector2Int((int)rotatedVec.x, (int)rotatedVec.y));
        }

        return rotated;
    }

    private Vector3 GetPlacementWorldCorner(BuildingData.Dir dir, Vector2Int origin)
    {
        Vector2 worldCorner = buildGrid.GetWorldPositionCorner(origin.x, origin.y);
        worldCorner += BuildingData.GetRotatedVector(dir, -0.5f, -0.5f) + new Vector2(0.5f, 0.5f);
        return worldCorner;
    }

    public Vector3 GetSnappedPlacementCorner(Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        Vector2Int origin = GetPlacementOrigin(worldPos, data, dir);
        return GetPlacementWorldCorner(dir, origin);
    }

    public bool TryRemoveBuildingAtWorldPosition(Vector3 worldPos, out PlacedBuilding removedBuilding)
    {
        removedBuilding = null;
        if (!buildGrid.TryGetGridObject(worldPos, out BuildCell firstCell) || firstCell.placedBuilding == null)
            return false;

        removedBuilding = firstCell.placedBuilding;
        List<Vector2Int> removedCells = new List<Vector2Int>();
        ClearBuildingCells(removedBuilding, removedCells);
        return removedCells.Count > 0;
    }
    public void ClearBuildingCells(PlacedBuilding building, List<Vector2Int> removedCells)
    {
        if (building == null || building.OccupiedPositions == null)
            return;

        foreach (var pos in building.OccupiedPositions)
        {
            BuildCell cell = buildGrid.GetGridObject(pos.x, pos.y);
            if (cell != null && cell.placedBuilding == building)
            {
                cell.ClearBuilding();
                removedCells.Add(pos);
            }
        }
    }
}
