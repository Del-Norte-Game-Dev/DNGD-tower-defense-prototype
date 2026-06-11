using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BuildingData;

public class BuildManager : GenericSingleton<BuildManager>
{
    [SerializeField] private BuildingRegistry buildingRegistry;
    [SerializeField] bool enableDebug = false;
    private BuildingData currentData;
    private int currentDataIndex = 0;

    private BuildMap buildMap;
    private bool isInitialized;
    private BuildPreview previewController;
    private BuildingData.Dir currentDir = BuildingData.Dir.Right;

    public event System.Action<PlacedBuilding> BuildingPlaced;
    public event System.Action<PlacedBuilding> BuildingRemoved;

    private Dictionary<Vector2Int, PlacedBuilding> surroundingCache = new Dictionary<Vector2Int, PlacedBuilding>();


    GridDebugDrawer debugDrawer;

    public void Initialize(BuildingRegistry registry, int width, int height, float cellSize, Vector3 origin)
    {
        if (isInitialized)
            return;

        buildingRegistry = registry;
        currentData = registry != null && registry.Count > 0 ? registry.Get(0) : null;
        currentDataIndex = 0;
        buildMap = new BuildMap(width, height, cellSize, origin);

        if (currentData != null)
        {
            previewController = new BuildPreview();
            previewController.Initialize(currentData, currentDir);
        }
        isInitialized = true;

        if (enableDebug)
        {
            GameObject debugObject = new GameObject("BuildGridDebug");
            debugDrawer = debugObject.AddComponent<GridDebugDrawer>();
            debugObject.transform.SetParent(transform);
            debugDrawer.Initialize(buildMap.BuildGrid);
            debugDrawer.HideAll();
            debugDrawer.SetText(true);
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Vector2 worldPos = Vector2.zero;

        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
        }

        previewController?.UpdatePreview(worldPos, buildMap, currentData, currentDir);

        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding(worldPos, currentData, currentDir);
        }

        if (Input.GetMouseButtonDown(1))
        {
            RemoveBuilding(worldPos);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentData = NextBuildingData();
        }
    }

    private BuildingData NextBuildingData(){
        currentDataIndex++;
        if (buildingRegistry == null || buildingRegistry.Count == 0) return null;
        return buildingRegistry.Get(currentDataIndex % buildingRegistry.Count);
    }

    public void PlaceBuilding(Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        if (data == null)
            return;

        if (PlacementService.TryPlace(buildMap, data, dir, worldPos, out PlacedBuilding placed))
        {
            BuildingPlaced?.Invoke(placed);
        }
    }

    public void RemoveBuilding(Vector3 worldPos)
    {
        if (buildMap.TryRemoveBuildingAtWorldPosition(worldPos, out PlacedBuilding removedBuilding))
        {
            removedBuilding.Remove();
            BuildingRemoved?.Invoke(removedBuilding);
        }
    }

    public void Rotate()
    {
        currentDir = BuildingData.GetNextDir(currentDir);
    }

    private void OnDestroy()
    {
        previewController?.Destroy();
    }

    //checks surronding buildings in a radius and returns a dictionary of their relative positions and transforms
    public Dictionary<Vector2Int, PlacedBuilding> GetSurroundingBuildingsAtPreview(int radius = 1)
    {

        surroundingCache.Clear();
        Vector2 center = previewController != null ? (Vector2)previewController.CenterPosition : Vector2.zero;
        Vector2 reversed = center - BuildingData.GetRotatedVector(currentDir, -0.5f, -0.5f) - new Vector2(0.5f, 0.5f);
        buildMap.BuildGrid.GetXY(reversed, out int xPos, out int yPos);
        Vector2Int cellPos = new Vector2Int(xPos, yPos);
        List<Vector2Int> footprint = BuildMap.GetRotatedFootprint(currentData.footprint, currentDir);

        List<Vector2Int> checkedOffsets = new List<Vector2Int>();


        foreach (Vector2Int offset in footprint)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = offset.x + x + cellPos.x;
                    int checkY = offset.y + y + cellPos.y;

                    Vector2Int checkPos = new Vector2Int(checkX, checkY);

                    // skip cells that are part of the same building
                    if (footprint.Contains(new Vector2Int(checkX - cellPos.x, checkY - cellPos.y))) continue;
                    
                    if (buildMap.BuildGrid.TryGetGridObject(checkX, checkY, out BuildCell neighborCell))
                    {
                        if (neighborCell.placedBuilding != null)
                            surroundingCache[new Vector2Int(checkX - cellPos.x, checkY - cellPos.y)] = neighborCell.placedBuilding;
                    }
                    checkedOffsets.Add(checkPos);
                    
                }
            }
        }

        return surroundingCache;
    }

    public Dictionary<Vector2Int, PlacedBuilding> GetSurroundingBuildingsAt(Vector3 worldPos, int radius = 1)
    {
        surroundingCache.Clear();

        buildMap.BuildGrid.GetXY(worldPos, out int xPos, out int yPos);
        Vector2Int cellPos = new Vector2Int(xPos, yPos);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = cellPos.x + x;
                int checkY = cellPos.y + y;

                if (buildMap.BuildGrid.TryGetGridObject(checkX, checkY, out BuildCell neighborCell))
                {
                    if (neighborCell.placedBuilding != null)
                        surroundingCache[new Vector2Int(x, y)] = neighborCell.placedBuilding;
                }
            }
        }

        //add preview building if it's within the radius and not already included
        if (previewController != null)
        {
            buildMap.BuildGrid.GetXY(previewController.CenterPosition, out int previewX, out int previewY);
            int dx = previewX - cellPos.x;
            int dy = previewY - cellPos.y;

            if (dx == 0 && dy == 0) { }  // same cell, skip
            else if (Mathf.Abs(dx) <= radius && Mathf.Abs(dy) <= radius)
            {
                // Only add if there isn't already a placed building at that offset
                var previewOffset = new Vector2Int(dx, dy);
                if (!surroundingCache.ContainsKey(previewOffset))
                    surroundingCache[previewOffset] = null;
            }
        }

        return surroundingCache;
    }
}
