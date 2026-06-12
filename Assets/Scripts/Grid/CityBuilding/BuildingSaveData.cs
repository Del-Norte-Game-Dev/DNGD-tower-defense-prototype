using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingSaveData
{
    public string buildingID;
    public Vector2Int origin;   // bottom-left position
    public int rotation;
}

[System.Serializable]
public class WorldSaveData
{
    public List<BuildingSaveData> buildings;
}