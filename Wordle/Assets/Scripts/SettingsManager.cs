using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

public class SettingsManager : MonoBehaviour
{
    // [Header("Elements")] 
    // [SerializeField] private Image soundsImage;
    // [SerializeField] private Image hapticsImage;
    //
    // [Header("Settings")] 
    // private bool soundsState;
    // private bool hapticsState;
    //
    // private void Start()
    // {
    //     LoadStates();
    // }
    //
    // public void SoundsButtonCallBack()
    // {
    //     soundsState = !soundsState;
    //     UpdateSoundsState();
    //     SaveStates();
    // }
    //
    // private void UpdateSoundsState()
    // {
    //     if (soundsState)
    //         EnableSounds();
    //     else
    //     {
    //         DisableSounds();
    //     }
    // }
    //
    // private void EnableSounds()
    // {
    //     SoundManager.Instance.EnableSounds();
    //     soundsImage.color = Color.white;
    // }
    //
    // private void DisableSounds()
    // {
    //     SoundManager.Instance.DisableSounds();
    //     soundsImage.color = Color.gray;
    // }
    //
    // public void HapticsButtonCallBack()
    // {
    //     hapticsState = !hapticsState;
    //     UpdateHapticsState();
    //     SaveStates();
    // }
    //
    // private void UpdateHapticsState()
    // {
    //     if (hapticsState)
    //         EnableHaptics();
    //     else
    //     {
    //         DisableHaptics();
    //     }
    // }
    //
    // private void EnableHaptics()
    // {
    //     hapticsImage.color = Color.white;
    // }
    //
    // private void DisableHaptics()
    // {
    //     hapticsImage.color = Color.gray;
    // }
    //
    // private void LoadStates()
    // {
    //     soundsState = PlayerPrefs.GetInt("SoundsState", 1) == 1;
    //     hapticsState = PlayerPrefs.GetInt("HapticsState", 1) == 1;
    //     
    //     UpdateSoundsState();
    //     UpdateHapticsState();
    // }
    //
    // private void SaveStates()
    // {
    //     PlayerPrefs.SetInt("SoundsState", soundsState ? 1 : 0);
    //     PlayerPrefs.SetInt("HapticsState", hapticsState ? 1 : 0);
    // }
    
    [Header("Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private ScrollRect currentBackgroundType;
    [SerializeField] private ScrollRect currentLanguage;

    private void Start()
    {
        LoadStates();
        currentBackgroundType.onValueChanged.AddListener(SetBackgroundType);
    }

    public void SetBackgroundType(Vector2 value)
    {
        PlayerPrefs.SetFloat("BackgroundType", value.x);
        SetDayAndNightBackground.Instance.SetBackgroundType(value.x);
    }
    
    public void SetLanguageType(Vector2 value)
    {
        int valueInt = Mathf.RoundToInt(value.x);
        string language = "";
        if (valueInt == 0)
        {
            language = "Spanish";
            MultiLanguage.Instance.Language(language);
        }
        else
        {
            language = "English";
            MultiLanguage.Instance.Language(language);
        }
        PlayerPrefs.SetString("Language", language);
    }

    private void LoadStates()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterVolumeSlider.value = masterVolume;
        SoundManager.Instance.SetMasterVolume(masterVolume);
        
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicVolumeSlider.value = musicVolume;
        SoundManager.Instance.SetMusicVolume(musicVolume);
        
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxVolumeSlider.value = sfxVolume;
        SoundManager.Instance.SetSFXVolume(sfxVolume);
        //hapticsState = PlayerPrefs.GetInt("HapticsState", 1) == 1;
        
        float scrollRectValue = PlayerPrefs.GetFloat("BackgroundType", 0f);
        currentBackgroundType.horizontalNormalizedPosition = scrollRectValue;
        Debug.Log(scrollRectValue);
        SetDayAndNightBackground.Instance.SetBackgroundType(scrollRectValue);
        
        string language = PlayerPrefs.GetString("Language", "Spanish");
        currentLanguage.horizontalNormalizedPosition = language == "Spanish" ? 0f : 1f;
        MultiLanguage.Instance.Language(language);
    }

    public void DebugScroll(float value)
    {
        Debug.Log("Value: " + value);
    }
    public void ChangeMasterVolume(float volume)
    {
        SoundManager.Instance.SetMasterVolume(volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void ChangeMusicVolume(float volume)
    {
        SoundManager.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void ChangeSFXVolume(float volume)
    {
        SoundManager.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}
