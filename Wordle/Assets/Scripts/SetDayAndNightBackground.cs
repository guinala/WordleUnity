using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;


public enum BackgroundType
{
    Sky,
    Nature,
}

public class SetDayAndNightBackground : MonoBehaviour
{
    public static SetDayAndNightBackground Instance;
    
    [Header("Background Music For Day")]
    [SerializeField] private AudioClip[] backgroundMusicSourcesDay;
    
    [Header("Background Music For Night")]
    [SerializeField] private AudioClip[] backgroundMusicSourcesNight;
    
    [Header("Elements")]
    public GameObject[] backgroundDaySky;
    public GameObject[] backgroundDayNature;
    public GameObject[] backgroundNightSky;
    public GameObject[] backgroundNightNature;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreTitleText;
    public TextMeshProUGUI titleText;
    
    [Header("Level Complete Texts")]
    [SerializeField] private TextMeshProUGUI[] levelCompleteTexts;
    
    [Header("Game Over Texts")]
    [SerializeField] private TextMeshProUGUI[] gameOverTexts;

    [Header("Day Hour")]
    public int dayHour = 6; 
    
    [Header("Night Hour")]
    public int nightHour = 19;

    private AudioSource audioSource;
    private int currentBackgroundIndex;
    private BackgroundType backgroundType = BackgroundType.Sky;
    private bool isDay = true;
    
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        int hour = DateTime.Now.Hour;
        
        if (hour >= dayHour && hour < nightHour)
        {
            DayMode();
        }
        else
        {
            NightMode();
        }
        
        audioSource.Play();
    }

    public void SetBackgroundType(float value)
    {
        int valueInt = Mathf.RoundToInt(value);
        Debug.Log("Valor a utilizar para el fondo: " +  valueInt);
        if(valueInt == 0)
        {
            backgroundType = BackgroundType.Sky;
            if (!isDay)
            {
                backgroundNightSky[currentBackgroundIndex].SetActive(true);
                backgroundNightNature[currentBackgroundIndex].SetActive(false);
            }
            else
            {
                backgroundDaySky[currentBackgroundIndex].SetActive(true);
                backgroundDayNature[currentBackgroundIndex].SetActive(false);
            }
        }
        else
        {
            backgroundType = BackgroundType.Nature;

            if (!isDay)
            {
                backgroundNightNature[currentBackgroundIndex].SetActive(true);
                backgroundNightSky[currentBackgroundIndex].SetActive(false);
            }
            else
            {
                backgroundDayNature[currentBackgroundIndex].SetActive(true);
                backgroundDaySky[currentBackgroundIndex].SetActive(false);
            }
        }
        
        
    }

    private void DayMode()
    {
        // if(backgroundDay != null) backgroundDay.SetActive(true);
        // if(backgroundNight != null) backgroundNight.SetActive(false);
        
        foreach(var bg in backgroundDaySky)
        {
            bg.SetActive(false);
        }
        foreach(var bg in backgroundDayNature)
        {
            bg.SetActive(false);
        }

        if (backgroundType == BackgroundType.Nature)
        {
            currentBackgroundIndex = Random.Range(0, backgroundDayNature.Length);
            GameObject selectedBackground = backgroundDayNature[currentBackgroundIndex];
            selectedBackground.SetActive(true);
        }
        else if (backgroundType == BackgroundType.Sky)
        {
            currentBackgroundIndex = Random.Range(0, backgroundDaySky.Length);
            GameObject selectedBackground = backgroundDaySky[Random.Range(0, backgroundDaySky.Length)];
            selectedBackground.SetActive(true);
        }
        if(scoreText != null) scoreText.color = Color.black;
        if(scoreTitleText != null) scoreTitleText.color = Color.black;
        if(titleText != null) titleText.color = Color.black;
        
        audioSource.clip = backgroundMusicSourcesDay[Random.Range(0, backgroundMusicSourcesDay.Length)];

        SetTextsDayMode();
        isDay = true;
    }

    private void NightMode()
    {
        // if(backgroundDay != null) backgroundDay.SetActive(false);
        // if(backgroundNight != null) backgroundNight.SetActive(true);
        foreach(var bg in backgroundNightSky)
        {
            bg.SetActive(false);
        }
        foreach(var bg in backgroundNightNature)
        {
            bg.SetActive(false);
        }
        if (backgroundType == BackgroundType.Nature)
        {
            currentBackgroundIndex = Random.Range(0, backgroundNightNature.Length);
            GameObject selectedBackground = backgroundNightNature[Random.Range(0, backgroundNightNature.Length)];
            selectedBackground.SetActive(true);
        }
        else if (backgroundType == BackgroundType.Sky)
        {
            currentBackgroundIndex = Random.Range(0, backgroundNightSky.Length);
            GameObject selectedBackground = backgroundNightSky[Random.Range(0, backgroundNightSky.Length)];
            selectedBackground.SetActive(true);
        }
        if(scoreText != null) scoreText.color = Color.yellow;
        if(scoreTitleText != null) scoreTitleText.color = Color.yellow;
        if(titleText != null) titleText.color = Color.yellow;
        
        audioSource.clip = backgroundMusicSourcesNight[Random.Range(0, backgroundMusicSourcesNight.Length)];
        
        SetTextsNightMode();
        isDay = false;
    }
    
    private void SetTextsDayMode()
    {
        foreach (var text in levelCompleteTexts)
        {
            text.color = Color.black;
        }
        
        foreach (var text in gameOverTexts)
        {
            text.color = Color.black;
        }
    }
    
    private void SetTextsNightMode()
    {
        foreach (var text in levelCompleteTexts)
        {
            text.color = Color.yellow;
        }
        
        foreach (var text in gameOverTexts)
        {
            text.color = Color.yellow;
        }
    }
}
