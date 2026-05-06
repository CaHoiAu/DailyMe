using UnityEngine;

public abstract class BaseMiniGameManager : MonoBehaviour
{
    public abstract void LoadMiniGame(ScriptableObject minigame);
    public abstract void ResetMiniGame();
    public abstract bool IsMiniGameCompleted();
}
