using UnityEngine;

public class ResourceBuilding : MonoBehaviour, IBuilding
{
    [SerializeField] private ResourceType resourceType; //CHANGE THIS LATER, RN ITS JUST A STRING TO PRINT
    public ResourceType CollectResource()
    {
        return resourceType;
    }
}
