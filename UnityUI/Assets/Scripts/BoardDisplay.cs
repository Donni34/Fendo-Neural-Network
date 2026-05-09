using Fendo.Logic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardDisplay : MonoBehaviour
{
    [Header("Zuweisungen")]
    public GameObject cellPrefab; //Zellen
    public GameObject wallPrefab;

    [Header("Einstellungen")]
    public float spacing = 1.1f; // Kleiner Puffer zwischen den Feldern

    private Board _logicBoard;
    private SpriteRenderer[,] _cellRenderers = new SpriteRenderer[7, 7];
    private SpriteRenderer[,] _hWallRenderers = new SpriteRenderer[6, 7];
    private SpriteRenderer[,] _vWallRenderers = new SpriteRenderer[7, 6];

    private (int, int)? clicked_cell = null;

    private CellState player = CellState.Player1;
    

    void Start()
    {
        // 1. Deine Logik-Klasse initialisieren
        _logicBoard = new Board(7);

        // 2. Das visuelle Gitter bauen
        GenerateGrid();

        // 3. Den ersten Stand anzeigen
        UpdateVisuals();
    }

    void GenerateGrid()
    {
        float offset = (7 - 1) * spacing / 2f;

        for (int i = 0; i < 7; i++) for (int j = 0; j < 7; j++)
        {
            float xPos = (j * spacing) - offset;
            float yPos = (-i * spacing) + offset;
            Vector3 cellPos = new Vector3(xPos, yPos, 0);

            GameObject newCell = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
            newCell.AddComponent<CellBehavior>().Init(i, j, this);
            newCell.name = $"Cell_{i}_{j}";
            _cellRenderers[i, j] = newCell.GetComponent<SpriteRenderer>();

            // Vertikale Wände
            if (j < 6) // Nur bis zur vorletzten Spalte
            {
                Vector3 vWallPos = cellPos + new Vector3(spacing / 2f, 0, 0);
                GameObject vWall = Instantiate(wallPrefab, vWallPos, Quaternion.identity, transform);
                vWall.AddComponent<WallBehavior>().InitWall(i, j, this, false);
                vWall.name = $"VWall_{i}_{j}";
                _vWallRenderers[i, j] = vWall.GetComponent<SpriteRenderer>();
            }

            // Horizontale Wände
            if (i < 6) 
            {
                Vector3 hWallPos = cellPos + new Vector3(0, -spacing / 2f, 0);
                GameObject hWall = Instantiate(wallPrefab, hWallPos, Quaternion.Euler(0, 0, 90), transform);
                hWall.AddComponent<WallBehavior>().InitWall(i, j, this, true);
                hWall.name = $"HWall_{i}_{j}";
                _hWallRenderers[i, j] = hWall.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void UpdateVisuals()
    {
        Matrix<bool> vision = clicked_cell is (int r, int c) ? _logicBoard.GetVisionFrom(r, c) : _logicBoard.GetVision(player);
        Matrix<CellState> board = _logicBoard.board;
        Matrix<bool> h_borders = _logicBoard.horizontal_borders;
        Matrix<bool> v_borders = _logicBoard.vertical_borders;
        for (int i = 0; i < 7; i++) for (int j = 0; j < 7; j++)
        {
            Color colour;
            switch (board[i, j])
            {
                case CellState.Player1:
                    colour = Color.grey;
                    break;
                case CellState.Player2:
                    colour = Color.orange;
                    break;
                case CellState.Empty:
                    colour = vision[i, j] ? Color.green : Color.white;
                    break;
                default:
                    colour = Color.red;
                    break;
            }
            _cellRenderers[i, j].color = colour;
            if (j<6) _vWallRenderers[i, j].color = v_borders[i, j+1] ? Color.black : Color.white;
            if (i<6) _hWallRenderers[i, j].color = h_borders[i+1, j] ? Color.black : Color.white;
        }
    }


    public void OnCellClicked(int r, int c)
    {
        clicked_cell = (r, c) == clicked_cell ? null : (r, c);
        Debug.Log($"Logik: Zelle bei {clicked_cell} geklickt.");
        UpdateVisuals();
    }

    public void OnWallClicked(int r, int c, bool isHorizontal)
    {
        string type = isHorizontal ? "Horizontal" : "Vertikal";
        Debug.Log($"Logik: {type} Wand bei {r}/{c} geklickt.");
        // Hier: _logicBoard.SetWall(r, c, isHorizontal);
        UpdateVisuals();
    }
}