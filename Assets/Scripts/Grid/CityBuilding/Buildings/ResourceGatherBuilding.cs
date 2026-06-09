using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ResourceGatherBuilding : MonoBehaviour, IBuilding
{
    private ResourceType resourceType;

    public void Init()
    {
        EnemyWaveManager.OnWaveCleared += CollectResources;
    }
    public bool CanPlace(Vector3 worldPos)
    {
        Dictionary<Vector2Int, Transform> neighbors = BuildManager.Instance.GetSurroundingBuildings(worldPos);

        foreach (Transform neighbor in neighbors.Values)
        {
            if(neighbor.TryGetComponent<ResourceBuilding>(out ResourceBuilding resource)){
                resourceType = resource.CollectResource();
                return true;
            }
        }

        return false;
    }

    private void CollectResources()
    {
        ResourceManager.Instance.CollectResources(resourceType, 1);
    }

}
