using System;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEditor.Tilemaps;
using UnityEngine;

public class WallBuilding : BuildingBehavior
{
    private bool isPlaced = false;
    private List<PlacedBuilding> lastOrientedNeighbors = new List<PlacedBuilding>();

    [SerializeField] private GameObject wallPostModel;
    [SerializeField] private GameObject wall1WayModel;
    [SerializeField] private GameObject wall2WayCornerModel;
    [SerializeField] private GameObject wall2WayLineModel;
    [SerializeField] private GameObject wall3WayModel;
    [SerializeField] private GameObject wall4WayModel;

    public override void Init()
    {
        isPlaced = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isPlaced)
        {
            GetCorrectModel();
            OrientNeighbors();
        }

    }

    public void GetCorrectModel()
    {
        GetCorrectModelAt(BuildManager.Instance.GetSurroundingBuildingsAtPreview());
    }

    // Now uses GetSurroundingBuildingsAt which already injects preview
    public void GetCorrectModelFromWorldPos()
    {
        GetCorrectModelAt(BuildManager.Instance.GetSurroundingBuildingsAt(transform.position));
    }

    public void OrientNeighbors()
    {
        var surrounding = BuildManager.Instance.GetSurroundingBuildingsAtPreview();
        var neighbors = new List<PlacedBuilding>(surrounding.Values);

        // Reset neighbors no longer adjacent
        foreach (var old in lastOrientedNeighbors)
        {
            if (old != null && !neighbors.Contains(old))
                old.Transform.GetComponent<WallBuilding>()?.GetCorrectModelFromWorldPos();
        }

        // Update current neighbors — GetSurroundingBuildingsAt handles preview injection
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
                neighbor.Transform.GetComponent<WallBuilding>()?.GetCorrectModelFromWorldPos();
        }

        lastOrientedNeighbors = neighbors;
    }

    public void OrientNeighborsFromWorldPos()
    {
        var surrounding = BuildManager.Instance.GetSurroundingBuildingsAt(transform.position);
        var neighbors = new List<PlacedBuilding>(surrounding.Values);

        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
                neighbor.Transform.GetComponent<WallBuilding>()?.GetCorrectModelFromWorldPos();
        }
    }

    private void GetCorrectModelAt(Dictionary<Vector2Int, PlacedBuilding> surrounding)
    {
        bool north = HasWallAt(surrounding, new Vector2Int(0, 1));
        bool east = HasWallAt(surrounding, new Vector2Int(1, 0));
        bool south = HasWallAt(surrounding, new Vector2Int(0, -1));
        bool west = HasWallAt(surrounding, new Vector2Int(-1, 0));

        int mask = (north ? 1 : 0) | (east ? 2 : 0) | (south ? 4 : 0) | (west ? 8 : 0);

        wallPostModel.SetActive(false);
        wall1WayModel.SetActive(false);
        wall2WayCornerModel.SetActive(false);
        wall2WayLineModel.SetActive(false);
        wall3WayModel.SetActive(false);
        wall4WayModel.SetActive(false);

        switch (mask)
        {
            case 0b0000: wallPostModel.SetActive(true); break;

            case 0b0001: Activate(wall1WayModel, -90f); break;
            case 0b0010: Activate(wall1WayModel, 0f); break;
            case 0b0100: Activate(wall1WayModel, 90f); break;
            case 0b1000: Activate(wall1WayModel, 180f); break;

            case 0b0101: Activate(wall2WayLineModel, 0f); break;
            case 0b1010: Activate(wall2WayLineModel, 90f); break;

            case 0b0011: Activate(wall2WayCornerModel, -90f); break;
            case 0b0110: Activate(wall2WayCornerModel, 0f); break;
            case 0b1100: Activate(wall2WayCornerModel, 90f); break;
            case 0b1001: Activate(wall2WayCornerModel, 180f); break;

            case 0b0111: Activate(wall3WayModel, 0f); break;
            case 0b1110: Activate(wall3WayModel, 90f); break;
            case 0b1101: Activate(wall3WayModel, 180f); break;
            case 0b1011: Activate(wall3WayModel, 270f); break;

            case 0b1111: wall4WayModel.SetActive(true); break;
        }
    }

    private void Activate(GameObject model, float yRotation)
    {
        model.SetActive(true);
        model.transform.localEulerAngles = new Vector3(0f, yRotation, 0f);
    }

    private bool HasWallAt(Dictionary<Vector2Int, PlacedBuilding> surrounding, Vector2Int offset)
    {
        if (!surrounding.TryGetValue(offset, out PlacedBuilding neighbor)) return false;
        if (neighbor == null) return true; // null = injected preview wall

        return neighbor.Transform.GetComponent<WallBuilding>() != null;
    }
}
