using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : GenericSingleton<ResourceManager>
{
    [SerializeField] private List<ResourceEntry> startingResources;

    public Dictionary<ResourceType, int> resourceStorage = new();

    public event Action<ResourceType, int> OnResourceChanged;

    override protected void Awake()
    {
        base.Awake();
        ResetInventory();
    }

    public void ResetInventory()
    {
        resourceStorage.Clear();

        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resourceStorage[type] = 0;
        }

        foreach (var entry in startingResources)
        {
            resourceStorage[entry.type] = entry.amount;
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


    public bool CanAfford(List<ResourceEntry> costs)
    {
        foreach (var cost in costs)
        {
            if (GetAmount(cost.type) < cost.amount)
                return false;
        }
        return true;
    }

    public bool SpendResources(List<ResourceEntry> costs)
    {
        if (!CanAfford(costs))
            return false;

        foreach (var cost in costs)
        {
            resourceStorage[cost.type] -= cost.amount;
            OnResourceChanged?.Invoke(cost.type, resourceStorage[cost.type]);
        }

        return true;
    }
}

public enum ResourceType
{
    Wood,
    Stone
}
