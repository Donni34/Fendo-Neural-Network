public class CellBehavior : Clickable
{
    protected override void OnMouseDown()
    {
        display.OnCellClicked(row, col);
    }
}