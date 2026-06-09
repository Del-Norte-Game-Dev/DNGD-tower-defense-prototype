using System.Collections.Generic;
using UnityEngine;

public class FlowField
{
    private Grid<MapCell> costGrid;        
    private Grid<FlowFieldCell> flowGrid;

    private FlowFieldCell destination;

    public FlowField(Grid<MapCell> costGrid)
    {
        this.costGrid = costGrid;

        flowGrid = new Grid<FlowFieldCell>(
            costGrid.GetWidth(),
            costGrid.GetHeight(),
            costGrid.GetCellSize(),
            costGrid.GetOriginPosition(),
            (g, x, y) => new FlowFieldCell(x, y)
        );
    }

    public void Regenerate()
    {
        GenerateIntegrationField(destination);
        GenerateFlowField();
    }

    public bool TryGenerate(Vector3 worldPosition)
    {
        if (!flowGrid.TryGetGridObject(worldPosition, out FlowFieldCell dest))
            return false;

        GenerateIntegrationField(dest);
        GenerateFlowField();

        return true;
    }

    private void ResetGrid()
    {
        for (int x = 0; x < flowGrid.GetWidth(); x++)
        {
            for (int y = 0; y < flowGrid.GetHeight(); y++)
            {
                var cell = flowGrid.GetGridObject(x, y);
                cell.integrationCost = int.MaxValue;
                cell.flowDirection = Vector2.zero;
            }
        }
    }

    private void GenerateIntegrationField(FlowFieldCell dest)
    {
        ResetGrid();

        destination = dest;
        destination.integrationCost = 0;

        Queue<FlowFieldCell> queue = new Queue<FlowFieldCell>();
        queue.Enqueue(destination);

        while (queue.Count > 0)
        {
            FlowFieldCell cur = queue.Dequeue();

            foreach (Vector2Int direction in Grid<FlowFieldCell>.DIR8_ManhattanBias)
            {
                if (!flowGrid.TryGetGridObject(cur.x + direction.x, cur.y + direction.y, out FlowFieldCell neighbor))
                    continue;

                MapCell mapCell = costGrid.GetGridObject(neighbor.x, neighbor.y);
                if (!mapCell.IsWalkable())
                    continue;

                int newCost = cur.integrationCost + mapCell.cost;
                if (newCost < neighbor.integrationCost)
                {
                    neighbor.integrationCost = newCost;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    private void GenerateFlowField()
    {
        for (int x = 0; x < flowGrid.GetWidth(); x++)
        {
            for (int y = 0; y < flowGrid.GetHeight(); y++)
            {
                var cell = flowGrid.GetGridObject(x, y);
                if (cell.integrationCost == int.MaxValue)
                {
                    cell.flowDirection = Vector2.zero;
                    continue;
                }

                FlowFieldCell best = null;
                int bestCost = cell.integrationCost;

                foreach (Vector2Int direction in Grid<FlowFieldCell>.DIR8_ManhattanBias)
                {
                    if (!flowGrid.TryGetGridObject(x + direction.x, y + direction.y, out FlowFieldCell neighbor))
                        continue;

                    if (neighbor.integrationCost < bestCost)
                    {
                        bestCost = neighbor.integrationCost;
                        best = neighbor;
                    }
                }

                if (best != null)
                {
                    Vector2 dir = new Vector2(best.x - cell.x, best.y - cell.y).normalized;
                    cell.flowDirection = dir;
                }
                else
                {
                    cell.flowDirection = Vector2.zero;
                }
            }
        }

        flowGrid.TriggerDebugFullRefresh();
    }

    public bool TryGetFlowDirection(Vector3 worldPosition, out Vector2 dir)
    {
        if (flowGrid.TryGetGridObject(worldPosition, out FlowFieldCell cell))
        {
            dir = cell.flowDirection;
            return true;
        }

        dir = Vector2.zero;
        return false;
    }

    public Vector2 GetFlowDirection(int x, int y)
    {
        return flowGrid.GetGridObject(x ,y).flowDirection;
    }

    public Vector3 GetDestinationWorld()
    {
        return flowGrid.GetWorldPositionCenter(destination.x, destination.y);
    }

    public Grid<FlowFieldCell> GetFlowGrid()
    {
        return flowGrid;
    }

    public Grid<MapCell> GetCostGrid()
    {
        return costGrid;
    }
}