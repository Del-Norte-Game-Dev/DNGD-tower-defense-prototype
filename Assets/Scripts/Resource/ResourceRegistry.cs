using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceRegistry", menuName = "Resource/ResourceRegistry")]
public class ResourceDatabase : ScriptableObject
{
    public List<Resource> resources;

    private Dictionary<ResourceType, Resource> lookup;

    public void Initialize()
    {
        lookup = new Dictionary<ResourceType, Resource>();

        foreach (var res in resources)
        {
            if (res == null) continue;
            lookup[res.type] = res;
        }
    }

    void OnValidate()
    {
        if (lookup == null)
            Initialize();
    }

    public Resource Get(ResourceType type)
    {
        return lookup.TryGetValue(type, out var res) ? res : null;
    }
}
