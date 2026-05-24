using UnityEngine;

public class FlowFieldCell
{
    public int x;
    public int y;

    public int integrationCost;
    public Vector2 flowDirection;

    public FlowFieldCell(int x, int y)
    {
        this.x = x;
        this.y = y;
        integrationCost = int.MaxValue;
        flowDirection = Vector2.zero;
    }

    public override string ToString()
    {
        return integrationCost.ToString();
    }
}