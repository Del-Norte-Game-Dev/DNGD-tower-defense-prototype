using UnityEngine;
using System;
using TMPro;

public class TextDisplay
{
    // Supports both TextMeshPro (3D) and TextMeshProUGUI (UI)
    public Component textComponent { get; private set; }
    public GameObject textObject { get; private set; }
    private Func<string> trackedProvider;

    internal TextDisplay(GameObject obj, Component txtComp, Func<string> provider)
    {
        textObject = obj;
        textComponent = txtComp;
        trackedProvider = provider;
    }

    public void UpdateTrackedText()
    {
        if (trackedProvider == null || textComponent == null) return;
        string s = trackedProvider();
        if (textComponent is TextMeshPro tmp)
            tmp.text = s;
        else if (textComponent is TextMeshProUGUI tmpui)
            tmpui.text = s;
    }

    public void SetUpdateTracker(Func<string> provider)
    {
        trackedProvider = provider;
    }

    public void UpdateText(string text)
    {
        if (textComponent == null) return;
        if (textComponent is TextMeshPro tmp)
            tmp.text = text;
        else if (textComponent is TextMeshProUGUI tmpui)
            tmpui.text = text;
    }

    public void UpdatePosition(Vector3 position)
    {
        if (textObject != null) textObject.transform.position = position;
    }
}
