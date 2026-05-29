using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Building/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    public List<BuildingData> data;
}
