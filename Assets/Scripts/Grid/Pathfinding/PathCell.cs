public class MapCell
{
    public int x { get; private set; }
    public int y { get; private set; }
    public byte cost; // cost to walk over this cell
    // other map cell data ex. tile type
    
    public MapCell(int x, int y, byte cost = 1)
    {
        this.x = x;
        this.y = y;
        SetCost(cost);
    }

    public void IncreaseCost(byte amount)
    {
        if (cost == byte.MaxValue) return;
        if (cost + amount > byte.MaxValue) {
            cost = byte.MaxValue;
        }
        else
        {
            cost += amount;
        }
    }

    public void SetCost(byte amount)
    {
        if (cost == amount) return;
        if (amount > byte.MaxValue) cost = byte.MaxValue;
        cost = amount;
    }

    public bool IsWalkable()
    {
        return cost < byte.MaxValue;
    }

    public override string ToString()
    {
        return cost.ToString();
    }
}
