using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : GenericSingleton<ResourceManager>
{
    public Dictionary<ResourceType, int> resourceStorage = new();

    public event Action<ResourceType, int> OnResourceChanged;

    override protected void Awake()
    {
        base.Awake();
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resourceStorage[type] = 0;
        }
    }

    public void CollectResources(ResourceType type, int amount)
    {
        resourceStorage[type] += amount;
        OnResourceChanged?.Invoke(type, resourceStorage[type]);
    }

    public int GetAmount(ResourceType type)
    {
        return resourceStorage.TryGetValue(type, out var val) ? val : 0;
    }
}

public enum ResourceType
{
    Wood,
    Stone
}
