using System.Collections.Generic;
using UnityEngine;

public class BuildPreview
{
    private GameObject preview;
    private Renderer[] previewRenderers;
    private BuildingData currentData;
    private BuildingData.Dir currentDir;

    public bool IsActive => preview != null && preview.activeSelf;
    public Vector3 CenterPosition => preview != null ? preview.transform.position : Vector3.zero;

    public void Initialize(BuildingData data, BuildingData.Dir dir)
    {
        currentData = data;
        currentDir = dir;
        CreatePreviewIfNeeded();
    }

    public void Destroy()
    {
        if (preview != null)
        {
            Object.Destroy(preview);
            preview = null;
            previewRenderers = null;
            currentData = null;
        }
    }

    public void UpdatePreview(Vector2 worldPos, BuildMap map, BuildingData data, BuildingData.Dir dir)
    {
        if (data == null || data.prefab == null)
        {
            if (preview != null)
                preview.SetActive(false);
            return;
        }

        if (preview == null || currentData != data || preview.name != data.prefab.name + "_Preview")
        {
            if (preview != null)
                Object.Destroy(preview);

            currentData = data;
            currentDir = dir;
            preview = Object.Instantiate(data.prefab);
            preview.name = data.prefab.name + "_Preview";
            RemovePreviewColliders(preview);
            previewRenderers = preview.GetComponentsInChildren<Renderer>();
            SetPreviewMaterials();
        }

        currentDir = dir;

        Vector3 snappedPos = map.GetSnappedPlacementCorner(worldPos, data, dir);

        bool canPlace = map.TryGetPlacement(worldPos, data, dir, out Vector3 pfPos);

        if (canPlace && data.prefab != null && data.prefab.TryGetComponent<IBuilding>(out IBuilding prefabBehavior))
        {
            canPlace = prefabBehavior.CanPlace(worldPos);
        }

        if (canPlace)
        {
            preview.transform.position = pfPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 1f, 1f, 0.5f));
        }
        else
        {
            preview.transform.position = snappedPos;
            preview.transform.rotation = Quaternion.Euler(0, 0, BuildingData.GetRotFromDir(dir));
            preview.SetActive(true);
            SetPreviewColor(new Color(1f, 0.25f, 0.25f, 0.5f));
        }
    }

    private void RemovePreviewColliders(GameObject target)
    {
        foreach (var collider in target.GetComponentsInChildren<Collider>())
        {
            Object.Destroy(collider);
        }

        foreach (var collider2D in target.GetComponentsInChildren<Collider2D>())
        {
            Object.Destroy(collider2D);
        }
    }

    private void SetPreviewMaterials()
    {
        if (previewRenderers == null)
            return;

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

    private void CreatePreviewIfNeeded()
    {
        if (currentData == null || currentData.prefab == null)
            return;

        if (preview == null)
        {
            preview = Object.Instantiate(currentData.prefab);
            preview.name = currentData.prefab.name + "_Preview";
            RemovePreviewColliders(preview);
            previewRenderers = preview.GetComponentsInChildren<Renderer>();
            SetPreviewMaterials();
        }
    }
}
