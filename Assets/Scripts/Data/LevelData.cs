using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public MiniGameEntry[] miniGames;
    //public GridMiniLevelData[] gridMiniLevel;
}