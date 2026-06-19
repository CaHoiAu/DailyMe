using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMiniGameManager : MonoBehaviour
{
    public abstract void LoadMiniGame(ScriptableObject minigame);
    public abstract void ResetMiniGame();
    public abstract bool IsMiniGameCompleted();
    public virtual void OnVerifyButtonClicked() { }
    public virtual Dictionary<string, GridObjectSnapshot> GetObjectStateSnapshot() => null;
    public virtual void ApplyObjectStateSnapshot(Dictionary<string, GridObjectSnapshot> snapshot) { }
}
