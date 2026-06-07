using System;
using UnityEngine;

public class FlowFieldManager : GenericSingleton<FlowFieldManager>
{
    private FlowField flowField;
    private FlowFieldVisual visual;
    private MapProvider mapProvider;
    [SerializeField] bool enableDebug = false;

    private Vector3 currentDestination;
    private bool costGridDirty;
    private float costGridDirtyTimer;
    [SerializeField] private float regenerateDelay = 0.05f;

    public void Initialize(MapProvider mapProvider, Vector3 worldDestination)
    {
        flowField = new FlowField(mapProvider.GetGrid());
        SetDestination(worldDestination);
        mapProvider.CostGridChanged += HandleCostGridChanged;

        if (!enableDebug) return;
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
        costGridDirty = true;
        costGridDirtyTimer = 0f;
    }

    private void Update()
    {
        if (!costGridDirty || flowField == null) return;

        costGridDirtyTimer += Time.deltaTime;
        if (costGridDirtyTimer < regenerateDelay) return;

        costGridDirty = false;
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