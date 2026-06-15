using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveRegistry", menuName = "Wave/WaveRegistry")]
public class WaveRegistry : ScriptableObject
{
    public List<WaveDefinition> waves;
}
