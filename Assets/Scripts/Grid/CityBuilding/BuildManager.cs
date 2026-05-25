using UnityEngine;

public class BuildManager : GenericSingleton<BuildManager>
{
    [SerializeField] private int width = 100;
    [SerializeField] private int height = 100;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [SerializeField] private BuildingData tempBuildingData;
    private BuildMap buildMap;
    private GameObject preview;
    private Renderer[] previewRenderers;
    private BuildingData.Dir currentDir = BuildingData.Dir.Right;

    GridDebugDrawer debugDrawer;

    protected override void Awake()
    {
        base.Awake();
        buildMap = new BuildMap(width, height, cellSize, origin);
        InitializePreview();

        GameObject debugObject = new GameObject("BuildGridDebug");
        debugDrawer = debugObject.AddComponent<GridDebugDrawer>();
        debugDrawer.Initialize(buildMap.BuildGrid);
        debugDrawer.HideAll();
        debugDrawer.SetText(true);
    }

    void Update()
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        UpdatePreview(worldPos);

        if (Input.GetMouseButtonDown(0))
        {
            PlaceBuilding(worldPos, tempBuildingData, currentDir);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }
    }

    private void InitializePreview()
    {
        if (tempBuildingData == null || tempBuildingData.prefab == null)
        {
            return;
        }

        if (preview != null)
        {
            Destroy(preview);
        }

        preview = Instantiate(tempBuildingData.prefab);
        preview.name = tempBuildingData.prefab.name + "_Preview";
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
        if (tempBuildingData == null || tempBuildingData.prefab == null)
        {
            if (preview != null)
            {
                preview.SetActive(false);
            }
            return;
        }

        if (preview == null || preview.name != tempBuildingData.prefab.name + "_Preview")
        {
            InitializePreview();
        }

        if (buildMap.TryGetPlacement(worldPos, tempBuildingData, currentDir, out Vector3 pfPos))
        {
            preview.transform.position = pfPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(currentDir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 1f, 1f, 0.5f));
        }
        else
        {
            preview.transform.position = worldPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(currentDir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 0.25f, 0.25f, 0.5f));
        }
    }

    public void PlaceBuilding(Vector3 worldPos, BuildingData data, BuildingData.Dir dir)
    {
        if (data == null)
        {
            return;
        }

        if (buildMap.TryPlaceBuilding(worldPos, data, dir, out Vector3 pfPos))
        {
            Instantiate(data.prefab, pfPos, Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir)));
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
}
