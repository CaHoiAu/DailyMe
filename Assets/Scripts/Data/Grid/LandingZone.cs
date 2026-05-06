using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class LandingZone : MonoBehaviour
{
    public string zoneId;
    public Sprite zoneSprite;
    public Color zoneColor = Color.white;
    public float zoneHeight = 1f;
    public float zoneWidth = 1f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ✅ Debug để xem tình trạng khi awake
        Debug.Log($"[LandingZone {zoneId}] Awake - SpriteRenderer: {spriteRenderer != null}, Sprite: {zoneSprite}, SpriteRenderer.sprite: {spriteRenderer?.sprite}", this);

        ApplyVisuals();
    }

    public void ApplyVisuals()
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[LandingZone {zoneId}] ApplyVisuals - SpriteRenderer is null!", this);
            return;
        }

        if (zoneSprite != null)
        {
            spriteRenderer.sprite = zoneSprite;
            spriteRenderer.color = zoneColor;
            transform.localScale = new Vector3(zoneWidth, zoneHeight, 1f);
            Debug.Log($"[LandingZone {zoneId}] Applied sprite: {zoneSprite.name}", this);
        }
        else
        {
            Debug.LogWarning($"[LandingZone {zoneId}] zoneSprite is NULL in inspector!", this);
        }
    }

    public void SetVisuals(Sprite sprite, Color color)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
                spriteRenderer.color = color;
                Debug.Log($"[LandingZone {zoneId}] SetVisuals applied: {sprite.name}", this);
            }
            else
            {
                Debug.LogWarning($"[LandingZone {zoneId}] SetVisuals - sprite is NULL!", this);
            }
        }
        else
        {
            Debug.LogError($"[LandingZone {zoneId}] SetVisuals - SpriteRenderer component not found!", this);
        }
    }
}
