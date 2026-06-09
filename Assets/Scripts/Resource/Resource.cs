using UnityEngine;

[CreateAssetMenu(menuName = "Resource/Resource")]
public class Resource : ScriptableObject
{
    public ResourceType type;

    public string displayName;
    public Sprite icon;

    public Color UIColor = Color.white;
}