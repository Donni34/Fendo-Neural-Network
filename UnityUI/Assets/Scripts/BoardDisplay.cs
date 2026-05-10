using Fendo.Logic;
using Fendo.Logic.enums;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BoardDisplay : MonoBehaviour
{
    [Header("Zuweisungen")]
    public GameObject cellPrefab; //Zellen
    public GameObject wallPrefab;

    [Header("Einstellungen")]
    public float cellWidth = 1f;
    public float wallWidth = .2f;
    public float freeSpace = .1f;

    private Board _logicBoard;
    private SpriteRenderer[,] _cellRenderers = new SpriteRenderer[7, 7];
    private SpriteRenderer[,] _hWallRenderers = new SpriteRenderer[6, 7];
    private SpriteRenderer[,] _vWallRenderers = new SpriteRenderer[7, 6];

    private (int, int)? first_cell = null;
    private (int, int)? second_cell = null;

    private CellState player = CellState.Player1;
    private List<Turn> turns = new List<Turn>();
    private int counter = 0;
    

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
        float spacing = cellWidth + wallWidth + freeSpace;
        float offset = (7 - 1) * spacing / 2f;

        Vector3 cellScale = new Vector3(cellWidth, cellWidth, 1);
        Vector3 wallScale = new Vector3(wallWidth, cellWidth, 1);

        for (int i = 0; i < 7; i++) for (int j = 0; j < 7; j++)
        {
            float xPos = (j * spacing) - offset;
            float yPos = (-i * spacing) + offset;
            Vector3 cellPos = new Vector3(xPos, yPos, 0);

            GameObject newCell = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
            newCell.transform.localScale = cellScale;
            newCell.AddComponent<CellBehavior>().Init(i, j, this);
            newCell.name = $"Cell_{i}_{j}";
            _cellRenderers[i, j] = newCell.GetComponent<SpriteRenderer>();

            // Vertikale Wände
            if (j < 6) // Nur bis zur vorletzten Spalte
            {
                Vector3 vWallPos = cellPos + new Vector3(spacing / 2f, 0, 0);
                GameObject vWall = Instantiate(wallPrefab, vWallPos, Quaternion.identity, transform);
                vWall.transform.localScale = wallScale;
                vWall.AddComponent<WallBehavior>().InitWall(i, j, this, false);
                vWall.name = $"VWall_{i}_{j}";
                _vWallRenderers[i, j] = vWall.GetComponent<SpriteRenderer>();
            }

            // Horizontale Wände
            if (i < 6) 
            {
                Vector3 hWallPos = cellPos + new Vector3(0, -spacing / 2f, 0);
                GameObject hWall = Instantiate(wallPrefab, hWallPos, Quaternion.Euler(0, 0, 90), transform);
                hWall.transform.localScale = wallScale;
                hWall.AddComponent<WallBehavior>().InitWall(i, j, this, true);
                hWall.name = $"HWall_{i}_{j}";
                _hWallRenderers[i, j] = hWall.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void UpdateVisuals()
    {
        Matrix<bool> vision = first_cell is (int r, int c) ? _logicBoard.GetVisionFrom(r, c) : _logicBoard.GetVision(player);
        Matrix<CellState> board = _logicBoard.board;
        Matrix<bool> h_borders = _logicBoard.horizontal_borders;
        Matrix<bool> v_borders = _logicBoard.vertical_borders;

        Color Pale(Color colour)
        {
            float paleness = .5f;
            return Color.Lerp(colour, Color.white, paleness);
        }

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
                    if (first_cell == (i, j))
                    {
                        if (player == CellState.Player1) colour = Pale(Color.grey);
                        else colour = Pale(Color.orange);
                    }
                    else if (second_cell == (i, j))
                    {
                        if (player == CellState.Player1) colour = Pale(Color.grey);
                        else colour = Pale(Color.orange);
                    }
                    break;
                default:
                    colour = Color.red;
                    break;
            }
            _cellRenderers[i, j].color = colour;

            if (j < 6)
            {
                if (v_borders[i, j+1]) colour = Color.black;
                else
                {
                    colour = Color.white;
                    if (second_cell is (int r2, int c2))
                    {
                        if (DetermineDirection(r2, c2, i, j, false) != Border.NaB) colour = Color.grey;
                    }
                }
                _vWallRenderers[i, j].color = colour;
            }
            if (i < 6)
            {
                if (h_borders[i+1, j]) colour = Color.black;
                else
                {
                    colour = Color.white;
                    if (second_cell is (int r2, int c2))
                    {
                        if (DetermineDirection(r2, c2, i, j, true) != Border.NaB) colour = Color.grey;
                    }
                }
                _hWallRenderers[i, j].color = colour;
            }
        }
    }


    public void OnCellClicked(int r, int c)
    {
        if (_logicBoard.board[r, c] == TogglePlayer(player))
        {
            first_cell = null;
            second_cell = null;
        }
        else if (first_cell == null)
        {
            first_cell = (r, c);
            if (_logicBoard.board[r, c] == CellState.Empty)
            {
                Place p = new Place(r, c, player);
                MakeTurn(p);
            }
        }
        else
        {
            if (second_cell != null)
            {
                first_cell = null;
                second_cell = null;
            }
            else second_cell = (r, c);
        }
        Debug.Log($"Logik: Zelle bei {first_cell} geklickt.");
        DebugOutput();
        UpdateVisuals();
    }
    public void OnWallClicked(int r, int c, bool isHorizontal)
    {
        if (first_cell is (int r1, int c1) && second_cell is (int r2, int c2))
        {
            Border border = DetermineDirection(r2, c2, r, c, isHorizontal);
            Move m = new Move(r1, c1, r2, c2, player, border);
            MakeTurn(m);
        }
        else
        {
            first_cell = null;
            second_cell = null; 
        }
        string type = isHorizontal ? "Horizontal" : "Vertikal";
        Debug.Log($"Logik: {type} Wand bei {r}/{c} geklickt.");
        DebugOutput();
        UpdateVisuals();
    }

    public Border DetermineDirection(int r_cell, int c_cell, int r_border, int c_border, bool isHorizontal)
    {
        if (isHorizontal && c_cell == c_border)
        {
            if (r_cell == r_border + 1) return Border.North;
            if (r_cell == r_border) return Border.South;
        }
        if (!isHorizontal && r_cell == r_border)
        {
            if (c_cell == c_border + 1) return Border.West;
            if (c_cell == c_border) return Border.East;
        }
        return Border.NaB;
    }

    public void MakeTurn(Turn t)
    {
        if (_logicBoard.MakeTurn(t))
        {
            turns.Append(t);
            player = TogglePlayer(player);
            counter++;
        }
        first_cell = null;
        second_cell = null;
    }

  
    private CellState TogglePlayer(CellState p)
    {
        return p switch
        {
            CellState.Player1 => CellState.Player2,
            CellState.Player2 => CellState.Player1,
            _ => p,
        };
    }

    private void DebugOutput()
    {
        Debug.Log($"Player: {player} FirstCell: {first_cell} SecondCell {second_cell}");
    }
}