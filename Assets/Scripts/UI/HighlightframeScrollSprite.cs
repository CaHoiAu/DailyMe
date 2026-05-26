using UnityEngine;
using UnityEngine.Rendering;

public class HighlightframeScrollSprite : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Material material;
    private SpriteRenderer spriteRenderer;
    private float offset;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError($"❌ HighlightframeScrollSprite: SpriteRenderer NOT found on {gameObject.name}");
            return;
        }

        Debug.Log($"✅ SpriteRenderer found on {gameObject.name}");
        Debug.Log($"   - Current Sprite: {spriteRenderer.sprite?.name}");
        Debug.Log($"   - Current Material: {spriteRenderer.material?.name}");

        // ✅ Kiểm tra Sprite có không
        if (spriteRenderer.sprite == null)
        {
            Debug.LogError($"❌ Sprite is NULL on {gameObject.name}!");
            return;
        }

        Debug.Log($"✅ Sprite found: {spriteRenderer.sprite.name}");
        Debug.Log($"   - Sprite Texture: {spriteRenderer.sprite.texture?.name}");

        // ✅ Tạo instance riêng của material
        material = Instantiate(spriteRenderer.material);
        spriteRenderer.material = material;

        if (material == null)
        {
            Debug.LogError($"❌ Material is NULL after Instantiate on {gameObject.name}");
            return;
        }

        Debug.Log($"✅ Material Instantiated: {material.name}");
        Debug.Log($"   - Shader: {material.shader.name}");

        // ✅ Lấy texture từ Sprite thay vì từ Material
        Texture spriteTexture = spriteRenderer.sprite.texture;
        if (spriteTexture == null)
        {
            Debug.LogError($"❌ Sprite Texture is NULL on {gameObject.name}!");
            return;
        }

        Debug.Log($"✅ Sprite Texture found: {spriteTexture.name}");
        Debug.Log($"   - Wrap Mode: {spriteTexture.wrapMode}");

        // ✅ Kiểm tra và sửa Wrap Mode nếu cần
        if (spriteTexture.wrapMode != TextureWrapMode.Repeat)
        {
            Debug.LogWarning($"⚠️ WARNING: Wrap Mode is {spriteTexture.wrapMode}, not Repeat!");
            spriteTexture.wrapMode = TextureWrapMode.Repeat;
            Debug.Log($"   - Auto-set to Repeat");
        }

        // ✅ Gán texture vào material
        material.SetTexture("_MainTex", spriteTexture);
        Debug.Log($"✅ Texture set to Material");

        Debug.Log($"✅ HighlightframeScrollSprite initialized successfully on {gameObject.name}");
    }

    private void Update()
    {
        if (material == null) 
        {
            Debug.LogWarning($"⚠️ Material is NULL!");
            return;
        }

        offset += scrollSpeed * Time.deltaTime;
        material.SetTextureOffset("_MainTex", new Vector2(offset, 0));

        // Debug log mỗi 1 giây
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"✅ Scrolling: Offset = {offset:F2}, scrollSpeed = {scrollSpeed}");
        }
    }
}
