using System.Collections.Generic;
using UnityEngine;

public static class PlacementService
{
    public static bool TryPlace(
        BuildMap map, 
        BuildingData data, 
        BuildingData.Dir dir, 
        Vector3 worldPos, 
        out PlacedBuilding placed)
    {
        placed = null;

        if (data == null || map == null)
            return false;

        if (!map.TryGetPlacementCells(worldPos, data, dir, out Vector3 pfPos, out List<BuildCell> cells))
            return false;

        // Allow prefab-provided placement checks (prefab component may provide placement constraints)
        if (data.prefab != null && data.prefab.TryGetComponent<IBuilding>(out IBuilding prefabBehavior))
        {
            if (!prefabBehavior.CanPlace(worldPos))
                return false;
        }

        if (!ResourceManager.Instance.SpendResources(data.cost))
            return false;

        GameObject instance = Object.Instantiate(data.prefab, pfPos, Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir)));
        if (instance.TryGetComponent<IBuilding>(out IBuilding runtimeBehavior))
            runtimeBehavior.Init();

        List<Vector2Int> positions = cells.ConvertAll(c => new Vector2Int(c.x, c.y));
        List<Vector2Int> costPositions = GetCostFootprintPositions(map, worldPos, data, dir);
        Vector2Int origin = map.GetPlacementOrigin(worldPos, data, dir);
        placed = new PlacedBuilding(data, instance.transform, positions, costPositions, map, origin, dir);
        bool placedOnMap = map.PlaceBuilding(placed);
        if (!placedOnMap)
        {
            Object.Destroy(instance);
            placed = null;
            return false;
        }

        return true;
    }

    private static List<Vector2Int> GetCostFootprintPositions(BuildMap map, Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        List<Vector2Int> costPositions = new List<Vector2Int>();
        if (data.costFootprint == null || data.costFootprint.Count == 0)
            return costPositions;

        Vector2Int origin = map.GetPlacementOrigin(worldPos, data, dir);
        foreach (Vector2Int offset in BuildMap.GetRotatedFootprint(data.costFootprint, dir))
        {
            costPositions.Add(new Vector2Int(origin.x + offset.x, origin.y + offset.y));
        }

        return costPositions;
    }
}
