using System;
using UnityEngine;

public class FlowFieldManager : GenericSingleton<FlowFieldManager>
{
    private FlowField flowField;
    private FlowFieldVisual visual;
    private MapProvider mapProvider;

    private Vector3 currentDestination;

    public void Initialize(MapProvider mapProvider, Vector3 worldDestination)
    {
        flowField = new FlowField(mapProvider.GetGrid());
        SetDestination(worldDestination);
        mapProvider.CostGridChanged += HandleCostGridChanged;

        visual = new GameObject("Flow Visual")
            .AddComponent<FlowFieldVisual>();

        visual.transform.SetParent(transform, false);
        visual.Initialize();
        visual.SetFlowField(flowField);
    }

    private void OnDisable()
    {
        if (mapProvider != null)
        {
            mapProvider.CostGridChanged -= HandleCostGridChanged;
        }
    }

    private void HandleCostGridChanged()
    {
        flowField.Regenerate();
    }

    private void SetDestination(Vector3 worldDestination)
    {
        currentDestination = worldDestination;
        flowField.TryGenerate(currentDestination);
    }

    public FlowField GetFlowField(){
        return flowField;
    }
}