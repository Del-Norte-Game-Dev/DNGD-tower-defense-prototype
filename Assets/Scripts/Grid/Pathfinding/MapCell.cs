public class MapCell
{
    public int x { get; private set; }
    public int y { get; private set; }
    public byte cost { get; private set;} // cost to walk over this cell
    public bool isWalkable { get; private set; } // explicit walkability state
    
    public MapCell(int x, int y, byte cost = 1, bool isWalkable = true)
    {
        this.x = x;
        this.y = y;
        this.isWalkable = isWalkable;
        SetCost(cost);
    }

    public void SetWalkable(bool value)
    {
        isWalkable = value;
    }

    public bool SetCost(byte amount)
    {
        if (cost == amount) return true;
        if (amount >= byte.MaxValue){ // overflow detection
            cost = byte.MaxValue;
            return false;
        }
        cost = amount;
        return true;
    }

    public bool IncreaseCost(byte amount)
    {
        if (amount == 0) return true;
        if (cost >= byte.MaxValue - amount){ // overflow detection
            cost = byte.MaxValue;
            return false;
        }
        cost += amount;
        return true;
    }

    public bool IsWalkable()
    {
        return isWalkable;
    }

    public override string ToString()
    {
        return cost.ToString();
    }
}
