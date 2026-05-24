using UnityEngine;

[CreateAssetMenu(fileName = "FlowFieldVisualConfig", menuName = "FlowFieldVisualConfig")]
public class FlowFieldVisualConfig : ScriptableObject
{
    public Sprite arrowSprite;
    public Sprite dotSprite;

    public Color minCostColor;
    public Color maxCostColor;
    public Color impassibleColor;
}
