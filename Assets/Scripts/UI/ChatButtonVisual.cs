using UnityEngine;
using UnityEngine.UI;

public class ChatButtonVisual : MonoBehaviour
{
    [Header("References")]
    public Button button;
    public Image image;

    [Header("Sprites")]
    public Sprite readySprite;
    public Sprite waitingSprite;

    public void SetWaiting()
    {
        Debug.Log($"[SetWaiting] ===== METHOD CALLED =====");
        Debug.Log($"[SetWaiting] this.gameObject.name = {gameObject.name}");
        Debug.Log($"[SetWaiting] this.gameObject.activeInHierarchy = {gameObject.activeInHierarchy}");

        if (image == null)
        {
            Debug.LogError("[SetWaiting] ❌ image is NULL!");
            return;
        }

        Debug.Log($"[SetWaiting] image != null, proceeding...");
        Debug.Log($"[SetWaiting] image.name: {image.name}");
        Debug.Log($"[SetWaiting] image.gameObject.activeInHierarchy: {image.gameObject.activeInHierarchy}");
        Debug.Log($"[SetWaiting] image.enabled: {image.enabled}");

        if (waitingSprite == null)
        {
            Debug.LogError("[SetWaiting] ❌ waitingSprite is NULL!");
            return;
        }

        Debug.Log($"[SetWaiting] Sprite BEFORE: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetWaiting] waitingSprite.name: {waitingSprite.name}");

        // Try to assign the sprite
        Debug.Log($"[SetWaiting] Attempting to assign sprite...");
        image.sprite = waitingSprite;
        Debug.Log($"[SetWaiting] Sprite assignment complete");

        // Verify it was assigned
        Debug.Log($"[SetWaiting] Sprite AFTER assignment: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetWaiting] image.sprite == waitingSprite: {image.sprite == waitingSprite}");

        // Explicitly set color to full opacity
        Color color = image.color;
        color.a = 1.0f;
        image.color = color;

        Debug.Log($"[SetWaiting] Color set to: {image.color}");

        // Critical: Rebuild the canvas and graphic
        image.SetAllDirty();
        Debug.Log($"[SetWaiting] Called SetAllDirty()");

        // Rebuild the layout if there's a parent LayoutGroup
        LayoutRebuilder.ForceRebuildLayoutImmediate(image.rectTransform);
        Debug.Log($"[SetWaiting] Called ForceRebuildLayoutImmediate()");

        Canvas.ForceUpdateCanvases();
        Debug.Log($"[SetWaiting] Called ForceUpdateCanvases()");

        // Final verification
        Debug.Log($"[SetWaiting] FINAL Sprite: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetWaiting] ===== METHOD COMPLETE =====");

        if (button != null)
        {
            button.interactable = false;
        }
    }

    public void SetReady()
    {
        Debug.Log($"[SetReady] ===== METHOD CALLED =====");
        Debug.Log($"[SetReady] this.gameObject.name = {gameObject.name}");
        Debug.Log($"[SetReady] this.gameObject.activeInHierarchy = {gameObject.activeInHierarchy}");

        if (image == null)
        {
            Debug.LogError("[SetReady] ❌ image is NULL!");
            return;
        }

        Debug.Log($"[SetReady] image != null, proceeding...");
        Debug.Log($"[SetReady] image.name: {image.name}");
        Debug.Log($"[SetReady] image.gameObject.activeInHierarchy: {image.gameObject.activeInHierarchy}");
        Debug.Log($"[SetReady] image.enabled: {image.enabled}");

        if (readySprite == null)
        {
            Debug.LogError("[SetReady] ❌ defaultSprite is NULL!");
            return;
        }

        Debug.Log($"[SetReady] Sprite BEFORE: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetReady] defaultSprite.name: {readySprite.name}");

        // Try to assign the sprite
        Debug.Log($"[SetReady] Attempting to assign sprite...");
        image.sprite = readySprite;
        Debug.Log($"[SetReady] Sprite assignment complete");

        // Verify it was assigned
        Debug.Log($"[SetReady] Sprite AFTER assignment: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetReady] image.sprite == defaultSprite: {image.sprite == readySprite}");

        // Explicitly set color to full opacity
        Color color = image.color;
        color.a = 1.0f;
        image.color = color;

        Debug.Log($"[SetReady] Color set to: {image.color}");

        // Critical: Rebuild the canvas and graphic
        image.SetAllDirty();
        Debug.Log($"[SetReady] Called SetAllDirty()");

        // Rebuild the layout if there's a parent LayoutGroup
        LayoutRebuilder.ForceRebuildLayoutImmediate(image.rectTransform);
        Debug.Log($"[SetReady] Called ForceRebuildLayoutImmediate()");

        Canvas.ForceUpdateCanvases();
        Debug.Log($"[SetReady] Called ForceUpdateCanvases()");

        // Final verification
        Debug.Log($"[SetReady] FINAL Sprite: {image.sprite?.name ?? "NULL"}");
        Debug.Log($"[SetReady] ===== METHOD COMPLETE =====");

        if (button != null)
        {
            button.interactable = true;
        }
    }
}
