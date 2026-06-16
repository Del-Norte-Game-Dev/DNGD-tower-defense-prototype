using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDefinition", menuName = "Wave/WaveDefinition")]
public class WaveDefinition : ScriptableObject
{
    public List<EnemySpawn> enemies;
}
