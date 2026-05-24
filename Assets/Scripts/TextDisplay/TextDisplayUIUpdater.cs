using UnityEngine;
using UnityEngine.EventSystems;
using System;
using UnityEngine.InputSystem;

// Handles UI click/drag events for UI Text objects.
public class TextDisplayUIUpdater : MonoBehaviour, IDraggableUI
{
    private TextDisplay displayer;
    private bool draggable = false;
    private Action onClick;
    private Vector2 dragOffset = Vector2.zero;

    public void Init(TextDisplay d, Func<string> trackedProvider)
    {
        displayer = d;
    }

    public void SetDraggable(bool value)
    {
        draggable = value;
    }

    public void SetOnClick(Action callback)
    {
        onClick = callback;
    }

    void Update()
    {
        if (displayer != null)
        {
            displayer.UpdateTrackedText();
        }

        if (Input.GetMouseButtonDown(0) && IsMouseOver(GetRect()))
        {
            OnClick();
        }
    }

    public void OnClick()
    {
        onClick?.Invoke();
    }

    public RectTransform GetRect()
    {
        return displayer.textObject.GetComponent<RectTransform>();
    }

    public void OnDragStart() {}

    public void OnDrag(Vector2 position)
    {
        if (!draggable) return;
        GetRect().anchoredPosition = position;
    }

    public void OnDragEnd() {}

    bool IsMouseOver(RectTransform rt)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            rt,
            Input.mousePosition
        );
    }
}
