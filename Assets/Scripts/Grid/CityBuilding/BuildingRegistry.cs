using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Building/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    [SerializeField] private List<BuildingData> data = new List<BuildingData>();

    public IReadOnlyList<BuildingData> Data => data;

    public int Count => data?.Count ?? 0;

    public BuildingData Get(int index)
    {
        if (data == null || index < 0 || index >= data.Count)
            return null;
        return data[index];
    }
}
