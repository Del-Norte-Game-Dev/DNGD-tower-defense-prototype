public class MapCell
{
    public int x { get; private set; }
    public int y { get; private set; }
    public byte cost; // cost to walk over this cell
    public byte baseCost; // stores the cost before building placement
    // other map cell data ex. tile type
    
    public MapCell(int x, int y, byte cost = 1)
    {
        this.x = x;
        this.y = y;
        baseCost = cost;
        SetCost(cost);
    }

    public void IncreaseBaseCost(byte amount)
    {
        if (baseCost == byte.MaxValue) return;
        if (baseCost + amount > byte.MaxValue) {
            baseCost = byte.MaxValue;
        }
        else
        {
            baseCost += amount;
        }
    }

    public void SetBaseCost(byte amount)
    {
        if (baseCost == amount) return;
        if (amount > byte.MaxValue) baseCost = byte.MaxValue;
        baseCost = amount;
    }

    public void SetCost(byte amount)
    {
        if (cost == amount) return;
        if (amount > byte.MaxValue) cost = byte.MaxValue;
        cost = amount;
    }

    public void ResetToOriginalCost()
    {
        SetCost(baseCost);
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
