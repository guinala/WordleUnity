using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

public class SettingsManager : MonoBehaviour
{
    [Serializable]
    private class ThemeOption
    {
        public ThemeConfig theme;
        public int coinCost;
        public int requiredBestScore;
        public string requiredAchievement;
    }

    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("General Settings")]
    [SerializeField] private ScrollRect currentBackgroundType;
    [SerializeField] private ScrollRect currentLanguage;

    [Header("Theme Settings")]
    [SerializeField] private ThemeManager themeManager;
    [SerializeField] private ScrollRect currentTheme;
    [SerializeField] private ThemeOption[] themeOptions;
    [SerializeField] private LetterContainer previewCell;
    [SerializeField] private Key previewKey;
    [SerializeField] private TextMeshProUGUI themeNameText;
    [SerializeField] private TextMeshProUGUI unlockStateText;

    private const string SelectedThemePrefKey = "SelectedThemeIndex";

    private void Start()
    {
        LoadStates();
        currentBackgroundType.onValueChanged.AddListener(SetBackgroundType);
        if (currentLanguage != null)
            currentLanguage.onValueChanged.AddListener(SetLanguageType);
        if (currentTheme != null)
            currentTheme.onValueChanged.AddListener(SetThemeType);
    }

    public void SetBackgroundType(Vector2 value)
    {
        PlayerPrefs.SetFloat("BackgroundType", value.x);
        SetDayAndNightBackground.Instance.SetBackgroundType(value.x);
    }

    public void SetLanguageType(Vector2 value)
    {
        int valueInt = Mathf.RoundToInt(value.x);
        string language = valueInt == 0 ? "Spanish" : "English";
        MultiLanguage.Instance.Language(language);
        PlayerPrefs.SetString("Language", language);
    }

    public void SetThemeType(Vector2 value)
    {
        int themeIndex = Mathf.Clamp(Mathf.RoundToInt(value.x * Mathf.Max(themeOptions.Length - 1, 0)), 0, Mathf.Max(themeOptions.Length - 1, 0));
        SelectTheme(themeIndex, false);
    }

    public void UnlockOrSelectCurrentTheme()
    {
        if (themeOptions == null || themeOptions.Length == 0)
            return;

        int index = GetCurrentThemeIndex();
        ThemeOption option = themeOptions[index];

        if (!IsThemeUnlocked(index))
        {
            if (!CanUnlock(option))
            {
                RefreshThemePreview(index);
                return;
            }

            DataManager.instance.RemoveCoins(option.coinCost);
            PlayerPrefs.SetInt(GetUnlockKey(index), 1);
        }

        SelectTheme(index, true);
    }

    public void SetAchievementUnlocked(string achievementId)
    {
        if (string.IsNullOrEmpty(achievementId))
            return;

        PlayerPrefs.SetInt($"AchievementUnlocked_{achievementId}", 1);
        TryUnlockAchievementThemes();
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

        float scrollRectValue = PlayerPrefs.GetFloat("BackgroundType", 0f);
        currentBackgroundType.horizontalNormalizedPosition = scrollRectValue;
        SetDayAndNightBackground.Instance.SetBackgroundType(scrollRectValue);

        string language = PlayerPrefs.GetString("Language", "Spanish");
        currentLanguage.horizontalNormalizedPosition = language == "Spanish" ? 0f : 1f;
        MultiLanguage.Instance.Language(language);

        TryUnlockAchievementThemes();
        LoadThemeSelection();
    }

    private void LoadThemeSelection()
    {
        if (themeOptions == null || themeOptions.Length == 0 || currentTheme == null)
            return;

        int savedIndex = Mathf.Clamp(PlayerPrefs.GetInt(SelectedThemePrefKey, 0), 0, themeOptions.Length - 1);

        if (!IsThemeUnlocked(savedIndex))
            savedIndex = 0;

        currentTheme.horizontalNormalizedPosition = themeOptions.Length > 1 ? (float)savedIndex / (themeOptions.Length - 1) : 0f;
        SelectTheme(savedIndex, false);
    }

    private void SelectTheme(int index, bool saveSelection)
    {
        if (themeOptions == null || themeOptions.Length == 0)
            return;

        index = Mathf.Clamp(index, 0, themeOptions.Length - 1);

        if (IsThemeUnlocked(index))
        {
            ThemeManager manager = themeManager != null ? themeManager : ThemeManager.Instance;
            if (manager != null)
                manager.ApplyTheme(themeOptions[index].theme);
            if (saveSelection)
                PlayerPrefs.SetInt(SelectedThemePrefKey, index);
        }

        RefreshThemePreview(index);
    }

    private void RefreshThemePreview(int index)
    {
        if (themeOptions == null || themeOptions.Length == 0)
            return;

        ThemeOption option = themeOptions[Mathf.Clamp(index, 0, themeOptions.Length - 1)];

        if (themeNameText != null && option.theme != null)
            themeNameText.text = option.theme.DisplayName;

        bool unlocked = IsThemeUnlocked(index);

        if (unlockStateText != null)
        {
            if (unlocked)
                unlockStateText.text = "Unlocked";
            else if (CanUnlock(option))
                unlockStateText.text = $"Unlock for {option.coinCost} coins";
            else
                unlockStateText.text = "Locked";
        }

        if (previewCell != null)
            previewCell.Preview(option.theme);

        if (previewKey != null)
            previewKey.Preview(option.theme);
    }

    private int GetCurrentThemeIndex()
    {
        if (themeOptions == null || themeOptions.Length == 0 || currentTheme == null)
            return 0;

        return Mathf.Clamp(Mathf.RoundToInt(currentTheme.horizontalNormalizedPosition * Mathf.Max(themeOptions.Length - 1, 0)), 0, themeOptions.Length - 1);
    }

    private bool IsThemeUnlocked(int index)
    {
        if (index <= 0)
            return true;

        return PlayerPrefs.GetInt(GetUnlockKey(index), 0) == 1;
    }

    private string GetUnlockKey(int index)
    {
        return $"ThemeUnlocked_{index}";
    }

    private bool CanUnlock(ThemeOption option)
    {
        if (option == null)
            return false;

        bool hasCoins = DataManager.instance.GetCoins() >= option.coinCost;
        bool bestScoreReached = option.requiredBestScore <= 0 || DataManager.instance.GetBestScore() >= option.requiredBestScore;

        bool achievementRequired = !string.IsNullOrEmpty(option.requiredAchievement);
        bool achievementReached = !achievementRequired ||
                                  PlayerPrefs.GetInt($"AchievementUnlocked_{option.requiredAchievement}", 0) == 1;

        bool requirementReached = bestScoreReached || achievementReached;
        return hasCoins && requirementReached;
    }

    private void TryUnlockAchievementThemes()
    {
        if (themeOptions == null)
            return;

        for (int i = 1; i < themeOptions.Length; i++)
        {
            ThemeOption option = themeOptions[i];
            bool scoreReached = option.requiredBestScore > 0 && DataManager.instance.GetBestScore() >= option.requiredBestScore;
            bool achievementReached = !string.IsNullOrEmpty(option.requiredAchievement) &&
                                      PlayerPrefs.GetInt($"AchievementUnlocked_{option.requiredAchievement}", 0) == 1;

            if (scoreReached || achievementReached)
                PlayerPrefs.SetInt(GetUnlockKey(i), 1);
        }
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
