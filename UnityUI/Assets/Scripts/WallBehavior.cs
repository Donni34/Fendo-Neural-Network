public class WallBehavior : Clickable
{
    public bool isHorizontal; 

    public void InitWall(int r, int c, BoardDisplay d, bool horizontal)
    {
        base.Init(r, c, d);
        isHorizontal = horizontal;
    }

    protected override void OnMouseDown()
    {
        display.OnWallClicked(row, col, isHorizontal);
    }
}