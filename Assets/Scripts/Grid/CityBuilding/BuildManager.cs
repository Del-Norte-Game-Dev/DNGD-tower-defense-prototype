using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private readonly List<PlacedBuilding> placedBuildings = new List<PlacedBuilding>();

    GridDebugDrawer debugDrawer;

    public void Initialize(BuildingRegistry registry, int width, int height, float cellSize, Vector3 origin)
    {
        if (isInitialized)
            return;

        buildingRegistry = registry;
        //currentData = registry != null && registry.Count > 0 ? registry.Get(0) : null;
        currentData = null;
        currentDataIndex = 0;
        buildMap = new BuildMap(width, height, cellSize, origin);
        placedBuildings.Clear();

        //if (currentData != null)
        //{
            previewController = new BuildPreview();
            previewController.Initialize(currentData, currentDir);
        //}
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

        if (EventSystem.current.IsPointerOverGameObject())
            return;

        previewController?.UpdatePreview(worldPos, buildMap, currentData, currentDir);

        if (currentData != null)
        {
            

            if (Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(worldPos, currentData, currentDir);
            }

            

            if (Input.GetKeyDown(KeyCode.R))
            {
                Rotate();
            }

            /*if (Input.GetKeyDown(KeyCode.E))
            {
                currentData = NextBuildingData();
            }*/
        }

        if (Input.GetMouseButtonDown(1))
        {
            RemoveBuilding(worldPos);
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
            placedBuildings.Add(placed);
            BuildingPlaced?.Invoke(placed);
        }
    }

    public bool PlaceBuildingAtOrigin(BuildingData data, BuildingData.Dir dir, Vector2Int origin)
    {
        if (data == null || buildMap == null)
            return false;

        if (!TryBuildPositions(origin, data, dir, out List<Vector2Int> occupiedPositions))
            return false;

        Vector3 pfPos = buildMap.GetPlacementWorldCorner(dir, origin);
        GameObject instance = Instantiate(data.prefab, pfPos, Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir)));
        if (instance.TryGetComponent<IBuilding>(out IBuilding runtimeBehavior))
            runtimeBehavior.Init();

        List<Vector2Int> costPositions = GetCostFootprintPositions(origin, data, dir);
        PlacedBuilding placed = new PlacedBuilding(data, instance.transform, occupiedPositions, costPositions, buildMap, origin, dir);
        if (!buildMap.PlaceBuilding(placed))
        {
            Destroy(instance);
            return false;
        }

        placedBuildings.Add(placed);
        BuildingPlaced?.Invoke(placed);
        return true;
    }

    public WorldSaveData ToWorldSaveData()
    {
        WorldSaveData save = new WorldSaveData
        {
            buildings = new List<BuildingSaveData>()
        };

        foreach (PlacedBuilding placed in placedBuildings)
        {
            if (placed == null)
                continue;

            save.buildings.Add(new BuildingSaveData
            {
                buildingID = placed.Data.ID,
                origin = placed.Origin,
                rotation = (int)placed.Direction
            });
        }

        return save;
    }

    public void RemoveBuilding(Vector3 worldPos)
    {
        if (buildMap.TryRemoveBuildingAtWorldPosition(worldPos, out PlacedBuilding removedBuilding))
        {
            if (removedBuilding != null)
                placedBuildings.Remove(removedBuilding);

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

    private bool TryBuildPositions(Vector2Int origin, BuildingData data, BuildingData.Dir dir, out List<Vector2Int> occupiedPositions)
    {
        occupiedPositions = new List<Vector2Int>();

        foreach (Vector2Int offset in BuildMap.GetRotatedFootprint(data.footprint, dir))
        {
            Vector2Int position = new Vector2Int(origin.x + offset.x, origin.y + offset.y);
            if (!buildMap.BuildGrid.TryGetGridObject(position.x, position.y, out BuildCell cell) || !cell.CanBuild())
            {
                occupiedPositions = null;
                return false;
            }

            occupiedPositions.Add(position);
        }

        return true;
    }

    private List<Vector2Int> GetCostFootprintPositions(Vector2Int origin, BuildingData data, BuildingData.Dir dir)
    {
        List<Vector2Int> costPositions = new List<Vector2Int>();
        if (data.costFootprint == null || data.costFootprint.Count == 0)
            return costPositions;

        foreach (Vector2Int offset in BuildMap.GetRotatedFootprint(data.costFootprint, dir))
        {
            costPositions.Add(new Vector2Int(origin.x + offset.x, origin.y + offset.y));
        }

        return costPositions;
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

    public BuildingData GetBuildingData()
    {
        return currentData;
    }

    public void SelectBuilding(BuildingData buildingData)
    {
        currentData = buildingData;
    }
}
