using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CutscenePlayer : MonoBehaviour
{
    [Header("References")]
    public CanvasGroup canvasGroup;
    public Image backgroundImage;
    public Image slidingBackgroundImage;
    public Image characterImage;
    public RectTransform characterContainer;

    [Header("Common Transition (plays on every level switch)")]
    public Image transitionImage;
    public Vector2 transitionStartPosition;
    public Vector2 transitionCoverPosition;
    public Vector2 transitionEndPosition;
    public float transitionDuration = 0.4f;

    [Header("Timing")]
    public float fadeDuration = 0.5f;

    private GameObject spawnedCharacter;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        gameObject.SetActive(false);
    }

    public void Play(CutsceneData data, Action onComplete)
    {
        gameObject.SetActive(true);
        StartCoroutine(PlayRoutine(data, onComplete));
    }

    private IEnumerator PlayRoutine(CutsceneData data, Action onComplete)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0f;

        bool hasCutscene = data != null && data.background != null;

        backgroundImage.enabled = false;
        characterImage.enabled = false;
        if (slidingBackgroundImage != null)
            slidingBackgroundImage.enabled = false;

        // Common transition: slide in to cover the screen
        yield return SlideTransition(transitionStartPosition, transitionCoverPosition);

        if (hasCutscene)
        {
            backgroundImage.sprite = data.background;
            backgroundImage.enabled = true;
            SetupCharacter(data);
            SetupSlidingBackground(data);

            yield return Fade(0f, 1f);

            if (data.slidingBackground != null)
                yield return SlideBackground(data);
            else
                yield return new WaitForSeconds(data.duration);

            yield return Fade(1f, 0f);
        }

        onComplete?.Invoke();

        // Common transition: slide out to reveal the next mini-game
        yield return SlideTransition(transitionCoverPosition, transitionEndPosition);
        CleanupCharacter();
        gameObject.SetActive(false);
    }
    private IEnumerator SlideTransition(Vector2 from, Vector2 to)
    {
        if (transitionImage == null)
            yield break;

        transitionImage.rectTransform.anchoredPosition = from;
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            transitionImage.rectTransform.anchoredPosition = Vector2.Lerp(from, to, t / transitionDuration);
            yield return null;
        }
        transitionImage.rectTransform.anchoredPosition = to;
    }
    private void SetupCharacter(CutsceneData data)
    {
        characterImage.enabled = false;

        if (data.characterPrefab != null)
        {
            spawnedCharacter = Instantiate(data.characterPrefab, characterContainer != null ? characterContainer : transform);

            RectTransform rt = spawnedCharacter.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = data.characterPosition;

            if (!string.IsNullOrEmpty(data.animationTrigger))
            {
                Animator animator = spawnedCharacter.GetComponent<Animator>();
                if (animator != null)
                    animator.SetTrigger(data.animationTrigger);
            }
        }
        else if (data.character != null)
        {
            characterImage.enabled = true;
            characterImage.sprite = data.character;
            characterImage.rectTransform.anchoredPosition = data.characterPosition;
        }
    }
    private void SetupSlidingBackground(CutsceneData data)
    {
        if (slidingBackgroundImage == null)
            return;

        if (data.slidingBackground != null)
        {
            slidingBackgroundImage.enabled = true;
            slidingBackgroundImage.sprite = data.slidingBackground;
            slidingBackgroundImage.rectTransform.anchoredPosition = data.slideStartPosition;
        }
        else
        {
            slidingBackgroundImage.enabled = false;
        }
    }

    private IEnumerator SlideBackground(CutsceneData data)
    {
        float t = 0f;
        while (t < data.duration)
        {
            t += Time.deltaTime;
            slidingBackgroundImage.rectTransform.anchoredPosition =
                Vector2.Lerp(data.slideStartPosition, data.slideEndPosition, t / data.duration);
            yield return null;
        }
        slidingBackgroundImage.rectTransform.anchoredPosition = data.slideEndPosition;
    }

    private void CleanupCharacter()
    {
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
            spawnedCharacter = null;
        }
    }

    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}
