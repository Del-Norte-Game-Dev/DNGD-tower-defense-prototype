using UnityEngine;

public class ResourceManager : GenericSingleton<ResourceManager>
{
    public int woodCount;
    public int stoneCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CollectResources(ResourceType resourceType, int amount)
    {
        switch(resourceType)
        {
            case ResourceType.Wood:
                // Collect wood resources
                woodCount += amount;    
                break;
            case ResourceType.Stone:
                // Collect stone resources
                stoneCount += amount;
                break;
        }
    }
}

public enum ResourceType
{
    Wood,
    Stone
}
