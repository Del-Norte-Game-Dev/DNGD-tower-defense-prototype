using UnityEngine;
using System;

// This component is attached to dynamically-updated text objects so they can call back into TextDisplayer.
public class TextDisplayUpdater : MonoBehaviour
{
    private TextDisplay displayer;
    private Action onClick;
    private bool draggable = false;
    private Vector3 dragOffset = Vector3.zero;
    private bool isDragging = false;

    public void Init(TextDisplay d, Func<string> trackedProvider)
    {
        displayer = d;

        if (displayer.textObject.GetComponent<Collider>() == null)
        {
            BoxCollider bc = displayer.textObject.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = new Vector3(1f, 1f, 0.1f);
        }
    }

    // Called by TextDisplayManager when creating objects
    public void Init(TextDisplay d)
    {
        displayer = d;
    }

    public void SetOnClick(Action callback)
    {
        onClick = callback;
    }

    public void SetDraggable(bool value)
    {
        draggable = value;
    }

    void Update()
    {
        if (displayer != null)
        {
            displayer.UpdateTrackedText();
        }

        if (displayer == null || displayer.textObject == null || !draggable)
        {
            if (Input.GetMouseButtonDown(0))
                HandleClick();
            return;
        }

        // Drag handling
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == displayer.textObject.transform)
                {
                    isDragging = true;
                    dragOffset = displayer.textObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
                    onClick?.Invoke();
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
            displayer.textObject.transform.position = mouseWorldPos + dragOffset;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandleClick()
    {
        if (displayer == null || displayer.textObject == null) return;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform == displayer.textObject.transform)
            {
                onClick?.Invoke();
            }
        }
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}
