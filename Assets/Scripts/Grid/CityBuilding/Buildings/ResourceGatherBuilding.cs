using System.Collections.Generic;
using UnityEngine;

public class ResourceGatherBuilding : BuildingBehavior
{
    private ResourceType resourceType;
    [SerializeField] private float maxHealth = 10f;

    public override void Init()
    {
        EnemyWaveManager.OnWaveCleared += CollectResources;
    }

    public override bool CanPlace(Vector3 worldPos)
    {
        Dictionary<Vector2Int, PlacedBuilding> neighbors = BuildManager.Instance.GetSurroundingBuildingsAtPreview();


        foreach (PlacedBuilding neighbor in neighbors.Values)
        {
            if(neighbor.Transform.TryGetComponent<ResourceBuilding>(out ResourceBuilding resource)){
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
