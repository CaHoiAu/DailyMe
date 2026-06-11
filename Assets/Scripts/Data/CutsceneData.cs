using UnityEngine;

[System.Serializable]
public class CutsceneData
{
    public Sprite background;
    [Header("Static character (used if Character Prefab is empty)")]
    public Sprite character;

    [Header("Animated character (overrides static sprite if set)")]
    public GameObject characterPrefab;
    public string animationTrigger;
    public Vector2 characterPosition;
    public float duration = 2f;

    [Header("Sliding background layer (optional, slides during cutscene)")]
    public Sprite slidingBackground;
    public Vector2 slideStartPosition;
    public Vector2 slideEndPosition;
}