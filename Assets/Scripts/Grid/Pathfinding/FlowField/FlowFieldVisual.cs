using UnityEngine;

public class FlowFieldVisual : MonoBehaviour
{
    private FlowFieldVisualConfig config;

    private FlowField flowField;
    private Grid<FlowFieldCell> flowGrid;
    private GridDebugDrawer debugDrawer;

    private DebugMode debugMode;

    public void Initialize()
    {
        config = GlobalAssets.FlowFieldVisual;

        GameObject go = new GameObject("DebugDrawer");
        go.transform.SetParent(transform, false);

        debugDrawer = go.AddComponent<GridDebugDrawer>();

        debugMode = DebugMode.Sprites;

        TextDisplayManager.NewUI(new Vector3(-600f, -300f, 0), 1f)
            .WithInitialText("Switch Debug Mode")
            .WithOnClick(() => NextDebugMode())
            .Build();
    }

    public void SetFlowField(FlowField flowField)
    {
        this.flowField = flowField;
        flowGrid = flowField.GetFlowGrid();

        debugDrawer.Initialize(flowGrid);
        UpdateFlowDebug();
    }

    public void NextDebugMode()
    {
        debugMode = (DebugMode)(((int)debugMode + 1) % 3);
        UpdateFlowDebug();
    }

    private void UpdateFlowDebug()
    {
        if (flowField == null) return;
        debugDrawer.SetColor(false);

        switch (debugMode)
        {
            case DebugMode.Off:
                debugDrawer.SetText(false);
                debugDrawer.SetSprites(false);
                break;

            case DebugMode.Text:
                debugDrawer.SetText(true);
                debugDrawer.SetSprites(false);
                break;

            case DebugMode.Sprites:
                debugDrawer.SetText(false);
                debugDrawer.SetSprites(true,
                    (x, y) =>
                    {
                        Vector2 dir = flowField.GetFlowDirection(x, y);
                        return dir == Vector2.zero
                            ? config.dotSprite
                            : config.arrowSprite;
                    },
                    (x, y) =>
                    {
                        Vector2 dir = flowField.GetFlowDirection(x, y);
                        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    });
                break;
        }
    }

    private enum DebugMode
    {
        Off,
        Sprites,
        Text
    }
}