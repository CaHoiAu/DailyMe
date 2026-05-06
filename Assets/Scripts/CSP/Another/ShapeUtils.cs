using Unity.VisualScripting;
using UnityEngine;

public static class ShapeUtils
{
    public static Vector2Int[] RotateShape(Vector2Int[] shape, int rotation)
    {
        Vector2Int[] rotatedShape = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            Vector2Int offset = shape[i];
            switch (rotation % 4)
            {
                case 0: // 0 degrees
                    rotatedShape[i] = offset;
                    break;
                case 1: // 90 degrees
                    rotatedShape[i] = new Vector2Int(-offset.y, offset.x);
                    break;
                case 2: // 180 degrees
                    rotatedShape[i] = new Vector2Int(-offset.x, -offset.y);
                    break;
                case 3: // 270 degrees
                    rotatedShape[i] = new Vector2Int(offset.y, -offset.x);
                    break;
            }
        }
        return Normalize(rotatedShape);
    }
    private static Vector2Int[] Normalize(Vector2Int[] shape)
    {
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        foreach (var offset in shape)
        {
            if (offset.x < minX) minX = offset.x;
            if (offset.y < minY) minY = offset.y;
        }
        Vector2Int[] normalizedShape = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            normalizedShape[i] = new Vector2Int(shape[i].x - minX, shape[i].y - minY);
        }
        return normalizedShape;
    }
    public static Vector2Int[] TranslateCells(Vector2Int[] cells, int x, int y)
    {
        Vector2Int[] result = new Vector2Int[cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            result[i] = new Vector2Int(cells[i].x + x, cells[i].y + y);
        }
        return result;
    }
}
