using System;
using System.Collections.Generic;

public class AStar
{
    private Grid<AStarCell> searchGrid;

    private PriorityQueue<AStarCell, int> openSet;

    public AStar(Grid<AStarCell> grid)
    {
        searchGrid = grid;
        openSet = new PriorityQueue<AStarCell, int>();
    }

    public List<AStarCell> Search(AStarCell startCell, AStarCell destination)
    {
        openSet = new PriorityQueue<AStarCell, int>();

        // reset start
        startCell.gCost = 0;
        startCell.hCost = Heuristic(startCell, destination);
        startCell.parent = null;

        openSet.Enqueue(startCell, startCell.fCost);

        while (openSet.Count > 0)
        {
            AStarCell curCell = openSet.Dequeue();

            if (curCell == destination)
            {
                return ReconstructPath(curCell);
            }

            List<AStarCell> neighbors =
                searchGrid.GetNeighbors(
                    curCell.pathCell.x,
                    curCell.pathCell.y,
                    Grid<AStarCell>.DIR8_DiagonalBias);

            foreach (AStarCell neighbor in neighbors)
            {
                if (neighbor.pathCell.cost >= byte.MaxValue)
                    continue;

                int tentativeG = curCell.gCost + neighbor.pathCell.cost;

                if (tentativeG < neighbor.gCost)
                {
                    neighbor.gCost = tentativeG;
                    neighbor.hCost = Heuristic(neighbor, destination);
                    neighbor.parent = curCell;

                    openSet.Enqueue(neighbor, neighbor.fCost);
                }
            }
        }

        return null;
    }

    private int Heuristic(AStarCell curCell, AStarCell destination)
    {
        return Math.Abs(curCell.pathCell.x - destination.pathCell.x)
             + Math.Abs(curCell.pathCell.y - destination.pathCell.y);
    }

    private List<AStarCell> ReconstructPath(AStarCell endCell)
    {
        List<AStarCell> path = new List<AStarCell>();

        AStarCell current = endCell;

        while (current != null)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }
}