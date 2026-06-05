using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BuildingData;

public class BuildManager : GenericSingleton<BuildManager>
{
    [SerializeField] private BuildingRegistry buildingRegistry;
    private BuildingData currentData;
    private int currentDataIndex = 0;

    private BuildMap buildMap;
    private bool isInitialized;
    private GameObject preview;
    private Renderer[] previewRenderers;
    private BuildingData.Dir currentDir = BuildingData.Dir.Right;
    private BuildingData previewData;

    public event System.Action<List<Vector2Int>> BuildingPlaced;
    public event System.Action<List<Vector2Int>> BuildingRemoved;

    private Dictionary<Vector2Int, Transform> surroundingCache = new Dictionary<Vector2Int, Transform>();


    GridDebugDrawer debugDrawer;

    public void Initialize(BuildingRegistry registry, int width, int height, float cellSize, Vector3 origin)
    {
        if (isInitialized)
            return;

        buildingRegistry = registry;
        currentData = registry != null && registry.data.Count > 0 ? registry.data[0] : null;
        currentDataIndex = 0;
        buildMap = new BuildMap(width, height, cellSize, origin);

        if (currentData != null)
            InitializePreview();

        GameObject debugObject = new GameObject("BuildGridDebug");
        debugDrawer = debugObject.AddComponent<GridDebugDrawer>();
        debugObject.transform.SetParent(transform);
        debugDrawer.Initialize(buildMap.BuildGrid);
        debugDrawer.HideAll();
        debugDrawer.SetText(true);

        isInitialized = true;
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

        UpdatePreview(worldPos);

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
        return buildingRegistry.data[currentDataIndex % buildingRegistry.data.Count];
    }

    #region preview
    private void InitializePreview()
    {
        if (currentData == null || currentData.prefab == null)
        {
            return;
        }

        if (preview != null)
        {
            Destroy(preview);
        }

        preview = Instantiate(currentData.prefab);
        preview.name = currentData.prefab.name + "_Preview";
        previewData = currentData;
        RemovePreviewColliders(preview);
        previewRenderers = preview.GetComponentsInChildren<Renderer>();
        SetPreviewMaterials(preview);
    }

    private void RemovePreviewColliders(GameObject target)
    {
        foreach (var collider in target.GetComponentsInChildren<Collider>())
        {
            Destroy(collider);
        }

        foreach (var collider2D in target.GetComponentsInChildren<Collider2D>())
        {
            Destroy(collider2D);
        }
    }

    private void SetPreviewMaterials(GameObject target)
    {
        foreach (Renderer renderer in previewRenderers)
        {
            if (renderer == null)
                continue;

            Material material = new Material(renderer.sharedMaterial);
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = 0.5f;
                material.color = color;
            }
            renderer.material = material;
        }
    }

    private void SetPreviewColor(Color color)
    {
        if (previewRenderers == null)
            return;

        foreach (Renderer renderer in previewRenderers)
        {
            if (renderer == null)
                continue;

            Material material = renderer.material;
            if (material != null && material.HasProperty("_Color"))
            {
                material.color = color;
            }
        }
    }

    private void UpdatePreview(Vector2 worldPos)
    {
        if (currentData == null || currentData.prefab == null)
        {
            if (preview != null)
            {
                preview.SetActive(false);
            }
            return;
        }

        if (preview == null || previewData != currentData || preview.name != currentData.prefab.name + "_Preview")
        {
            InitializePreview();
        }

        Vector3 snappedPos = buildMap.GetSnappedPlacementCorner(worldPos, currentData, currentDir);

        if (buildMap.TryGetPlacement(worldPos, currentData, currentDir, out Vector3 pfPos))
        {
            preview.transform.position = pfPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(currentDir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 1f, 1f, 0.5f));
        }
        else
        {
            preview.transform.position = snappedPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(currentDir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 0.25f, 0.25f, 0.5f));
        }
    }
    #endregion

    public void PlaceBuilding(Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        if (data == null)
        {
            return;
        }

        if (buildMap.TryGetPlacementCells(worldPos, data, dir, out Vector3 pfPos, out List<BuildCell> cells))
        {
            GameObject instance = Instantiate(data.prefab, pfPos, Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir)));

            if (instance.TryGetComponent<IBuilding>(out IBuilding building))
            {
                building.Init();
            }


            if (buildMap.PlaceBuilding(instance.transform, cells))
            {
                BuildingPlaced?.Invoke(cells.ConvertAll(cell => new Vector2Int(cell.x, cell.y)));
            }
        }
    }

    public void RemoveBuilding(Vector3 worldPos)
    {
        if (buildMap.TryRemoveBuildingAtWorldPosition(worldPos, out Transform removedBuilding, out List<Vector2Int> removedCells))
        {
            Destroy(removedBuilding.gameObject);
            BuildingRemoved?.Invoke(removedCells);
        }
    }

    public void Rotate()
    {
        currentDir = BuildingData.GetNextDir(currentDir);
    }

    private void OnDestroy()
    {
        if (preview != null)
        {
            Destroy(preview);
        }
    }

    //checks surronding buildings in a radius and returns a dictionary of their relative positions and transforms
    public Dictionary<Vector2Int, Transform> GetSurroundingBuildings(Vector3 worldPos, int radius = 1)
    {

        surroundingCache.Clear();
        Vector2 reversed = (Vector2)preview.transform.position - BuildingData.GetRotatedVector(currentDir, -0.5f, -0.5f) - new Vector2(0.5f, 0.5f);
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
                        //Debug.Log(checkX + " , " + checkY + "  |  " + neighborCell.placedBuilding);
                        if (neighborCell.placedBuilding != null)
                            surroundingCache[new Vector2Int(checkX - cellPos.x, checkY - cellPos.y)] = neighborCell.placedBuilding;
                    }
                                        checkedOffsets.Add(checkPos);
                    
                }
            }
        }

        return surroundingCache;
    }
}
