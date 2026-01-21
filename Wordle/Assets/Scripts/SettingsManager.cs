using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Elements")] 
    [SerializeField] private Image soundsImage;
    [SerializeField] private Image hapticsImage;

    [Header("Settings")] 
    private bool soundsState;
    private bool hapticsState;

    private void Start()
    {
        LoadStates();
    }

    public void SoundsButtonCallBack()
    {
        soundsState = !soundsState;
        UpdateSoundsState();
        SaveStates();
    }

    private void UpdateSoundsState()
    {
        if (soundsState)
            EnableSounds();
        else
        {
            DisableSounds();
        }
    }

    private void EnableSounds()
    {
        SoundManager.Instance.EnableSounds();
        soundsImage.color = Color.white;
    }

    private void DisableSounds()
    {
        SoundManager.Instance.DisableSounds();
        soundsImage.color = Color.gray;
    }
    
    public void HapticsButtonCallBack()
    {
        hapticsState = !hapticsState;
        UpdateHapticsState();
        SaveStates();
    }
    
    private void UpdateHapticsState()
    {
        if (hapticsState)
            EnableHaptics();
        else
        {
            DisableHaptics();
        }
    }
    
    private void EnableHaptics()
    {
        hapticsImage.color = Color.white;
    }
    
    private void DisableHaptics()
    {
        hapticsImage.color = Color.gray;
    }

    private void LoadStates()
    {
        soundsState = PlayerPrefs.GetInt("SoundsState", 1) == 1;
        hapticsState = PlayerPrefs.GetInt("HapticsState", 1) == 1;
        
        UpdateSoundsState();
        UpdateHapticsState();
    }
    
    private void SaveStates()
    {
        PlayerPrefs.SetInt("SoundsState", soundsState ? 1 : 0);
        PlayerPrefs.SetInt("HapticsState", hapticsState ? 1 : 0);
    }
}
