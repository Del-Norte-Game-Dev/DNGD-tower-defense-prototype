using UnityEngine;

public static class GlobalAssets
{
    private static AssetLibrary instance;

    public static AssetLibrary Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<AssetLibrary>("AssetLibrary");
            }

            return instance;
        }
    }

    public static FlowFieldVisualConfig FlowFieldVisual =>
        Instance.flowFieldVisual;
}