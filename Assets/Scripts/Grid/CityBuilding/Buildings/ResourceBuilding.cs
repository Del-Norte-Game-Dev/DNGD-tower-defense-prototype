using UnityEngine;

public class ResourceBuilding : MonoBehaviour, IBuilding
{
    [SerializeField] private string resourceType; //CHANGE THIS LATER, RN ITS JUST A STRING TO PRINT
    public string CollectResource()
    {
        return resourceType;
    }
}
