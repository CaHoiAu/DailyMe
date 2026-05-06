using UnityEngine;

public class GridBoardManager : MonoBehaviour
{
    public int boardWidth = 5;
    public int boardHeight = 5;
    public float cellSize = 1f;
    public Vector3 origin;

    private LineRenderer lineRenderer;
    public bool showGrid = true;
    public Color gridColor = Color.white;
    public float gridLineWidth = 0.05f;

    public void SetUp(GridMiniLevelData data)
    {
        boardHeight = data.boardHeight;
        origin = data.boardOrigin;
        boardWidth = data.boardWidth;
        cellSize = data.cellSize;

        if (showGrid)
            DrawGridWithLine();
    }
    private void DrawGridWithLine()
    {
        GameObject gridParent = new GameObject("GridLines");
        gridParent.transform.SetParent(transform);

        for (int i = 0; i <= boardWidth; i++)
        {
            Vector3 start = origin + new Vector3(i * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(i * cellSize, boardHeight * cellSize, 0);
            CreateLine(start, end, gridParent.transform);
        }
        for (int u = 0; u <= boardHeight; u++)
        {
            Vector3 start = origin + new Vector3(0, u * cellSize, 0);
            Vector3 end = origin + new Vector3(boardWidth * cellSize, u* cellSize, 0);
            CreateLine(start, end, gridParent.transform);
        }
    }
    private void CreateLine(Vector3 start, Vector3 end, Transform parent)
    {
        GameObject obj = new GameObject("Line");
        obj.transform.SetParent(parent);

        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        lr.startWidth = gridLineWidth;
        lr.endWidth = gridLineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridColor;
        lr.endColor = gridColor;
    }
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }
    public Vector3 GridToWorld(Vector2Int cell)
    {
        return origin + new Vector3(cellSize * (cell.x + 0.5f), cellSize * (cell.y + 0.5f), 0f);
    }
    public bool IsInsideBoard(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < boardWidth && cell.y >= 0 && cell.y < boardHeight;
    }
    public bool IsInsideBoard(Vector2Int[] cells)
    {
        foreach (var cell in  cells)
        {
            if (!IsInsideBoard(cell))
                return false;
        }
        return true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int i = 0; i <= boardWidth; i++)
        {
            Vector3 start = origin + new Vector3(i * cellSize, 0, 0);
            Vector3 end = origin + new Vector3(i * cellSize, boardHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
        for (int u = 0; u <= boardHeight; u++)
        {
            Vector3 start = origin + new Vector3(0, u * cellSize, 0);
            Vector3 end = origin + new Vector3(boardWidth * cellSize, u* cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}
