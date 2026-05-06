using UnityEngine;

[CreateAssetMenu(fileName = "Grid Level Data", menuName = "Game/Grid Level Data")]
public class GridLevelData : ScriptableObject
{
    public GridMiniLevelData[] miniLevels;
}
