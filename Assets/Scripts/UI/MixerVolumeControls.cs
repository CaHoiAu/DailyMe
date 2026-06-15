using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Gắn vào panel Settings. Kéo AudioMixer asset (đã expose 2 parameter
/// "MusicVolume" và "SFXVolume") + 2 slider vào Inspector.
/// Mọi AudioSource trong game (setup thủ công, không cần script gì thêm)
/// chỉ cần set Output = group Music/SFX trong Mixer là sẽ chịu ảnh hưởng
/// bởi 2 slider này.
/// </summary>
public class MixerVolumeControls : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";
    private const string MUSIC_PREF_KEY = "MusicVolumePref";
    private const string SFX_PREF_KEY = "SfxVolumePref";

    private void Start()
    {
        float savedMusic = PlayerPrefs.GetFloat(MUSIC_PREF_KEY, 0.75f);
        float savedSfx = PlayerPrefs.GetFloat(SFX_PREF_KEY, 1f);

        // Set giá trị slider trước, không trigger callback
        musicSlider.SetValueWithoutNotify(savedMusic);
        sfxSlider.SetValueWithoutNotify(savedSfx);

        ApplyMusicVolume(savedMusic);
        ApplySfxVolume(savedSfx);

        musicSlider.onValueChanged.AddListener(OnMusicChanged);
        sfxSlider.onValueChanged.AddListener(OnSfxChanged);
    }

    private void OnMusicChanged(float value)
    {
        ApplyMusicVolume(value);
        PlayerPrefs.SetFloat(MUSIC_PREF_KEY, value);
    }

    private void OnSfxChanged(float value)
    {
        ApplySfxVolume(value);
        PlayerPrefs.SetFloat(SFX_PREF_KEY, value);
    }

    // Slider value 0-1 -> decibel cho Audio Mixer (-80dB = im lặng, 0dB = bình thường)
    private void ApplyMusicVolume(float value) =>
        audioMixer.SetFloat(MUSIC_PARAM, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);

    private void ApplySfxVolume(float value) =>
        audioMixer.SetFloat(SFX_PARAM, Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);

    private void OnDestroy()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }
}