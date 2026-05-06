using System.Collections.Generic;
using System.Xml.Xsl;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int rows = 4;
    public int columns = 8;
    public float cellSize = 1f;
    public Vector2 origin = Vector2.zero;

    private Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();

    private void Awake()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        cells.Clear();
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector2 worldPos = new Vector2(
                    origin.x + c * cellSize + cellSize * 0.5f,
                    origin.y + r * cellSize + cellSize * 0.5f
                );
                var id = new Vector2Int(r, c);
                cells[id] = new Cell(id, worldPos);
            }
        }
    }

    public Cell GetCell(Vector2Int id)
    {
        return cells.TryGetValue(id, out var cell) ? cell : null;
    }

    // Tìm cell gần nhất theo world position (ví dụ position của chuột khi thả object)
    public Cell GetNearestCell(Vector2 worldPosition)
    {
        int c = Mathf.FloorToInt((worldPosition.x - origin.x) / cellSize);
        int r = Mathf.FloorToInt((worldPosition.y - origin.y) / cellSize);

        var id = new Vector2Int(c, r);
        return GetCell(id);
    }
    private void OnDrawGizmos()
    {
        // Vẽ grid để debug trong Scene view
        Gizmos.color = new Color(1, 1, 1, 0.15f);

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Vector3 center = new Vector3(
                    origin.x + c * cellSize + cellSize * 0.5f,
                    origin.y + r * cellSize + cellSize * 0.5f,
                    0
                );

                Gizmos.DrawWireCube(center, new Vector3(cellSize, cellSize, 0.01f));
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
public class Cell
{
    public Vector2Int id; //(row, col)
    public Vector2 worldPosition;
    public bool isOccupied;
    public GameObject occupiedObject;

    public Cell(Vector2Int id, Vector2 worldPosition)
    {
        this.id = id;
        this.worldPosition = worldPosition;
        this.isOccupied = false;
        this.occupiedObject = null;
    }
}
