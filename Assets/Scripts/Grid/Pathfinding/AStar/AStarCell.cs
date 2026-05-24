public class AStarCell
{
    public MapCell pathCell;

    public int gCost; // cost from origin

    public int hCost; // estimate heuristic cost

    public int fCost => (gCost + hCost); // f = g + h

    public AStarCell parent;

    public AStarCell(MapCell pathCell)
    {
        this.pathCell = pathCell;
        Reset();
    }

    public AStarCell(int x, int y, byte cost = 1)
    {
        this.pathCell = new MapCell(x, y, cost);
        Reset();
    }

    public void Reset()
    {
        gCost = 0;
        hCost = 0;
    }

    public bool IsWalkable()
    {
        return pathCell.IsWalkable();
    }

    public override string ToString()
    {
        return fCost.ToString();
    }
}
