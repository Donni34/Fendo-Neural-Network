using UnityEngine;

public abstract class Clickable : MonoBehaviour
{
    public int row;
    public int col;
    protected BoardDisplay display;

    public void Init(int r, int c, BoardDisplay d)
    {
        row = r;
        col = c;
        display = d;
    }

    protected abstract void OnMouseDown();
}