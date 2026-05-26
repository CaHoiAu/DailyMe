using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HighlightStep
{
    public Image overlayImage;      // overlay riêng cho step này
    public Color overlayColor = new Color(1f, 0.9f, 0f, 0.3f);
    public float minAlpha = 0f;
    public float maxAlpha = 0.5f;
    public float speed = 2f;
}

public class OnBoardingHighLight : MonoBehaviour
{
    public List<HighlightStep> steps;
    private int currentStep = 0;
    private Coroutine pulseCoroutine;
    public string saveKey = "OnboardingCompleted";

    void Start()
    {
        if (PlayerPrefs.GetInt(saveKey, 0) == 0)
            ShowStep(0);
    }

    public void ShowStep(int index)
    {
        StopCurrentPulse();

        // Ẩn tất cả overlay
        foreach (var step in steps)
            if (step.overlayImage != null)
                step.overlayImage.gameObject.SetActive(false);

        if (index >= steps.Count)
        {
            CompleteOnboarding();
            return;
        }

        currentStep = index;
        var current = steps[index];

        if (current.overlayImage != null)
        {
            current.overlayImage.gameObject.SetActive(true);
            pulseCoroutine = StartCoroutine(PulseCoroutine(current));
        }
    }

    public void NextStep()
    {
        ShowStep(currentStep + 1);
    }

    private IEnumerator PulseCoroutine(HighlightStep step)
    {
        while (true)
        {
            float t = Mathf.PingPong(Time.time * step.speed, 1f);
            float alpha = Mathf.Lerp(step.minAlpha, step.maxAlpha, t);
            step.overlayImage.color = new Color(
                step.overlayColor.r,
                step.overlayColor.g,
                step.overlayColor.b,
                alpha
            );
            yield return null;
        }
    }

    private void StopCurrentPulse()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }

    public void CompleteOnboarding()
    {
        StopCurrentPulse();
        foreach (var step in steps)
            if (step.overlayImage != null)
                step.overlayImage.gameObject.SetActive(false);

        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();
    }

    [ContextMenu("Reset Onboarding")]
    public void ResetOnboarding() => PlayerPrefs.DeleteKey(saveKey);
}