using UnityEngine;

public class BuildCell
{
    public Grid<BuildCell> grid;
    public int x;
    public int y;

    public Transform placedBuilding;

    public BuildCell(Grid<BuildCell> grid, int x, int y, Transform placedBuilding = null)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.placedBuilding = placedBuilding;
    }

    public void SetBuilding(Transform building)
    {
        placedBuilding = building;
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
