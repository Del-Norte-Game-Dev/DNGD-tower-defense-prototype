using System.Collections.Generic;
using UnityEngine;

public class FlowFieldManager : GenericSingleton<FlowFieldManager>
{
    private Dictionary<Vector2Int, FlowField> cache = new();

    private FlowFieldVisual visual;

    public void Start()
    {
        visual = new GameObject("Flow Visual")
            .AddComponent<FlowFieldVisual>();

        visual.transform.SetParent(transform, false);
        visual.Initialize();
    }

    public FlowField GetOrCreateFlowField(Vector3 worldDestination)
    {
        Vector2Int key = WorldToKey(worldDestination);

        if (cache.TryGetValue(key, out FlowField field))
        {
            SetLatest(field);
            return field;
        }

        field = BuildFlowField(worldDestination);
        cache[key] = field;

        SetLatest(field);
        return field;
    }

    private FlowField BuildFlowField(Vector3 destination)
    {
        FlowField field = new FlowField(MapProvider.Instance.GetGrid());

        if (!field.TryGenerate(destination))
        {
            Debug.LogWarning("destination out of grid");
        }

        return field;
    }

    private void SetLatest(FlowField field)
    {
        visual?.SetFlowField(field);
    }

    private Vector2Int WorldToKey(Vector3 worldPos)
    {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x),
            Mathf.RoundToInt(worldPos.y)
        );
    }
}