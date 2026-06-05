using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class ResourceGatherBuilding : MonoBehaviour, IBuilding
{
    [SerializeField] private string resourceType; //CHANGE THIS LATER, RN ITS JUST A STRING TO PRINT

    public void Init()
    {

    }
    public bool CanPlace(Vector3 worldPos)
    {
        Dictionary<Vector2Int, Transform> neighbors = BuildManager.Instance.GetSurroundingBuildings(worldPos);

        foreach (Transform neighbor in neighbors.Values)
        {
            if(neighbor.TryGetComponent<ResourceBuilding>(out ResourceBuilding resource)){
                return true;
            }
        }

        return false;
    }

}
