using Fendo.Logic; 
using UnityEngine;

public class BoardDisplay : MonoBehaviour
{
    [Header("Zuweisungen")]
    public GameObject cellPrefab; // Hier ziehen wir gleich das Prefab rein

    [Header("Einstellungen")]
    public float spacing = 1.1f; // Kleiner Puffer zwischen den Feldern

    private Board _logicBoard;
    private SpriteRenderer[,] _cellRenderers = new SpriteRenderer[7, 7];

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
        // Wir berechnen die halbe Größe des Spielfelds
        // Bei 7 Spalten und 'spacing' Abstand ist die Mitte bei (7-1) * spacing / 2
        float offset = (7 - 1) * spacing / 2f;

        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                // Wir ziehen den Offset ab, um das Gitter zu zentrieren
                float xPos = (j * spacing) - offset;
                float yPos = (-i * spacing) + offset; // +offset, damit Reihe 0 oben ist

                Vector3 position = new Vector3(xPos, yPos, 0);

                GameObject newCell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                newCell.name = $"Cell_{i}_{j}";
                _cellRenderers[i, j] = newCell.GetComponent<SpriteRenderer>();
            }
        }
    }

    public void UpdateVisuals()
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                // Beispiel: Hol den Status aus deiner Board-Klasse
                // Ich nehme an, du hast sowas wie board.GetCellState(i,j)
                // Hier fiktives Beispiel:
                // var state = _logicBoard.GetCell(i, j); 

                // Wir färben es erst mal nur testweise ein:
                _cellRenderers[i, j].color = Color.white;

                // Wenn du gültige Züge hast, färbe sie grün:
                // if (_logicBoard.IsValidMove(i, j)) _cellRenderers[i,j].color = Color.green;
            }
        }
    }
}