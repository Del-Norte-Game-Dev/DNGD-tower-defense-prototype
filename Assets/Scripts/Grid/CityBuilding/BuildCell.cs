using UnityEngine;

public class BuildCell
{
    public Grid<BuildCell> grid;
    public int x;
    public int y;

    public PlacedBuilding placedBuilding;

    public BuildCell(Grid<BuildCell> grid, int x, int y, PlacedBuilding placedBuilding = null)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.placedBuilding = placedBuilding;
    }

    public void SetBuilding(PlacedBuilding building)
    {
        placedBuilding = building;
        grid.TriggerDebugRefresh(x, y);
    }

    public void ClearBuilding()
    {
        placedBuilding = null;
        grid.TriggerDebugRefresh(x, y);
    }

    public bool CanBuild()
    {
        return placedBuilding == null;
    }

    public override string ToString()
    {
        return CanBuild().ToString();
    }
}
