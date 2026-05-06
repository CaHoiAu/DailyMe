using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HighlightframeScroll : MonoBehaviour
{
    public float scrollSpeed = 0.5f;
    private Material material;
    private float offset;

    private void Start()
    {
        Image img = GetComponent<Image>();

        material = Instantiate(img.material);
        img.material = material;
    }
    private void Update()
    {
        offset += scrollSpeed * Time.deltaTime;
        material.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}
