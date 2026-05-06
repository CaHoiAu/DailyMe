using UnityEngine;

public class PlacementValue
{
    public int x;
    public int y;
    public int rotation; // 0: default, 1: 90 degrees, 2: 180 degrees, etc.
    public int stateIndex;
    public int layer;

    public PlacementValue(int x, int y, int rotation, int stateIndex, int layer)
    {
        this.x = x;
        this.y = y;
        this.rotation = rotation;
        this.stateIndex = stateIndex;
        this.layer = layer;
    }
    public override string ToString()
    {
        return $"({x},{y})_R{rotation}_S{stateIndex}_L{layer}";
    }
}
