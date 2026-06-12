using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Building/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    [SerializeField] private List<BuildingData> data = new List<BuildingData>();

    private Dictionary<string, BuildingData> lookup;

    public IReadOnlyList<BuildingData> Data => data;

    public int Count => data?.Count ?? 0;

    private void OnEnable()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, BuildingData>();

        foreach (var b in data)
        {
            if (b == null || string.IsNullOrEmpty(b.ID))
            {
                Debug.LogWarning("Invalid BuildingData in registry");
                continue;
            }

            if (lookup.ContainsKey(b.ID))
            {
                Debug.LogError($"Duplicate Building ID: {b.ID}");
                continue;
            }

            lookup.Add(b.ID, b);
        }
    }

    public BuildingData Get(int index)
    {
        if (data == null || index < 0 || index >= data.Count)
            return null;
        return data[index];
    }

    public BuildingData Get(string id)
    {
        if (lookup == null) BuildLookup();

        if (lookup.TryGetValue(id, out var result))
            return result;

        Debug.LogError($"Building ID not found: {id}");
        return null;
    }
}
