using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDragManager : GenericSingleton<UIDragManager>
{
    private IDraggableUI current;
    private Vector2 offset;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            current = FindDraggableUnderMouse();

            if (current != null)
            {
                RectTransform rt = current.GetRect();

                Vector2 local;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rt.parent as RectTransform,
                    Input.mousePosition,
                    null,
                    out local
                );

                offset = rt.anchoredPosition - local;

                current.OnDragStart();
            }
        }

        if (current != null && Input.GetMouseButton(0))
        {
            RectTransform rt = current.GetRect();

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt.parent as RectTransform,
                Input.mousePosition,
                null,
                out pos
            );

            current.OnDrag(pos + offset);
        }

        if (current != null && Input.GetMouseButtonUp(0))
        {
            current.OnDragEnd();
            current = null;
        }
    }

    private EventSystem GetOrCreateEventSystem()
    {
        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
        }

        return EventSystem.current;
    }

    private IDraggableUI FindDraggableUnderMouse()
    {
        EventSystem ev = GetOrCreateEventSystem();
        PointerEventData pointerData = new PointerEventData(ev);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        ev.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            IDraggableUI draggable = result.gameObject.GetComponentInParent<IDraggableUI>();
            if (draggable != null)
                return draggable;
        }

        return null;
    }
}

public interface IDraggableUI
{
    public RectTransform GetRect();
    public void OnDragStart();
    public void OnDrag(Vector2 newPos);
    public void OnDragEnd();
}