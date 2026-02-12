using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("Elements")]
    [SerializeField] private CanvasGroup menu_canvasGroup;
    [SerializeField] private CanvasGroup game_canvasGroup;
    [SerializeField] private CanvasGroup levelComplete_canvasGroup;
    [SerializeField] private CanvasGroup gameOver_canvasGroup;
    [SerializeField] private CanvasGroup settings_canvasGroup;
    [SerializeField] private CanvasGroup keyboard_canvasGroup;
    [SerializeField] private CanvasGroup loading_canvasGroup;
    [SerializeField] private CanvasGroup hint_canvasGroup;
    [SerializeField] private CanvasGroup mainPanelHint_canvasGroup;
    [SerializeField] private CanvasGroup givenHintPanel_canvasGroup;
    [SerializeField] private GameObject wordsContainer;
    
    [Header("Menu Elements")]
    [SerializeField] private TextMeshProUGUI menuCoins;
    [SerializeField] private TextMeshProUGUI menuBestScore;
    
    [Header("Level Complete Elements")]
    [SerializeField] private TextMeshProUGUI levelCompleteCoins;
    [SerializeField] private TextMeshProUGUI levelCompleteScore;
    [SerializeField] private TextMeshProUGUI levelCompleteSecretWord;
    [SerializeField] private TextMeshProUGUI levelCompleteBestScore;
    
    [Header("Game Over Elements")]
    [SerializeField] private TextMeshProUGUI gameOverCoins;
    [SerializeField] private TextMeshProUGUI gameOverSecretWord;
    [SerializeField] private TextMeshProUGUI gameOverBestScore;
    
    [Header("Game Elements")]
    [SerializeField] private TextMeshProUGUI gameScore;
    [SerializeField] private TextMeshProUGUI gameCoins;
    [SerializeField] private CanvasGroup countdown_canvasGroup;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private Color countdownNormalColor = Color.white;
    [SerializeField] private Color countdownCriticalColor = Color.red;
    
    [Header("Loading Elements")]
    [SerializeField] private TextMeshProUGUI loadingText;

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
        ShowMenu();
        HideGame();
        HideLevelComplete();
        HideGameOver();
        HideCountdownUI();
        GameManager.OnGameStateChanged += GameStateChangedCallback;
        DataManager.OnCoinsChanged += UpdateCoinsTexts;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        DataManager.OnCoinsChanged -= UpdateCoinsTexts;
    }

    private void ShowGame()
    {
        gameScore.text = DataManager.instance.GetScore().ToString();
        gameCoins.text = DataManager.instance.GetCoins().ToString();
        wordsContainer.SetActive(true);
        InputManager.instance.Initialize();
        ShowCanvasGroup(game_canvasGroup);
        ShowCanvasGroup(keyboard_canvasGroup);
        HideCountdownUI();
    }
    
    private void ShowLevelComplete()
    {
        levelCompleteCoins.text = DataManager.instance.GetCoins().ToString();
        levelCompleteScore.text = DataManager.instance.GetScore().ToString();
//        levelCompleteBestScore.text = DataManager.instance.GetBestScore().ToString();
        levelCompleteSecretWord.text = WordManager.instance.GetSecretWord();
        ShowCanvasGroup(levelComplete_canvasGroup);
    }

    private void ShowGameOver()
    {
        gameOverCoins.text = DataManager.instance.GetCoins().ToString();
        gameOverBestScore.text = DataManager.instance.GetBestScore().ToString();
        gameOverSecretWord.text = WordManager.instance.GetSecretWord();
        ShowCanvasGroup(gameOver_canvasGroup);
    }
    
    public void ShowLoading(bool hint)
    {
        string language = PlayerPrefs.GetString("Language", "Spanish");
        if(hint)
            loadingText.text = language == "Spanish" ? "Generando pista..." : "Generating Hint...";
        else
        {
            loadingText.text = language == "Spanish" ? "Generando palabra..." : "Generating Word...";
        }
        ShowCanvasGroup(loading_canvasGroup);
    }
    
    public void HideLoading()
    {
        HideCanvasGroup(loading_canvasGroup);
    }
    
    public void ShowSettings()
    {
        ShowCanvasGroup(settings_canvasGroup);
    }
    
    public void HideSettings()
    {
        HideCanvasGroup(settings_canvasGroup);
    }
    
    public void ShowHintUI()
    {
        ShowCanvasGroup(hint_canvasGroup);
        HideGivenHintPanel();
        HideMainHintPanel();
    }
    
    public void HideHintUI()
    {
        HideCanvasGroup(hint_canvasGroup);
    }
    
    public void ShowMainHintPanel()
    {
        ShowCanvasGroup(mainPanelHint_canvasGroup);
    }
    
    public void HideMainHintPanel()
    {
        HideCanvasGroup(mainPanelHint_canvasGroup);
    }
    
    public void ShowGivenHintPanel()
    {
        ShowCanvasGroup(givenHintPanel_canvasGroup);
    }
    
    public void HideGivenHintPanel()
    {
        HideCanvasGroup(givenHintPanel_canvasGroup);
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Menu:
                ShowMenu();
                HideGame();
                break;
            
            case GameState.Game:
                ShowGame();
                HideMenu();
                HideLevelComplete();
                HideGameOver();
                break;
            
            case GameState.LevelComplete:
                ShowLevelComplete();
                HideGame();
                break;
            
            case GameState.GameOver:
                ShowGameOver();
                HideGame();
                break;
        }
    }

    public void UpdateCoinsTexts()
    {
        menuCoins.text = DataManager.instance.GetCoins().ToString();
        gameCoins.text = DataManager.instance.GetCoins().ToString();
        levelCompleteCoins.text = DataManager.instance.GetCoins().ToString();
        gameOverCoins.text = DataManager.instance.GetCoins().ToString();
    }

    private void ShowMenu()
    {
        menuCoins.text = DataManager.instance.GetCoins().ToString();
        menuBestScore.text = DataManager.instance.GetBestScore().ToString();
        wordsContainer.SetActive(false);
        ShowCanvasGroup(menu_canvasGroup);
        HideCanvasGroup(keyboard_canvasGroup);
    }
    
    private void HideMenu()
    {
        HideCanvasGroup(menu_canvasGroup);
    }
    
    private void HideGame()
    {
        wordsContainer.SetActive(false);
        HideCanvasGroup(keyboard_canvasGroup);
        HideCanvasGroup(game_canvasGroup);
        HideCountdownUI();
    }
    
    private void HideLevelComplete()
    {
        HideCanvasGroup(levelComplete_canvasGroup);
    }

    private void HideGameOver()
    {
        HideCanvasGroup(gameOver_canvasGroup);
    }
    
    public void UpdateCountdownUI(float remainingSeconds, bool isCritical)
    {
        if (countdownText == null || countdown_canvasGroup == null)
            return;

        TimeSpan time = TimeSpan.FromSeconds(Mathf.CeilToInt(Mathf.Max(0f, remainingSeconds)));
        countdownText.text = $"{time.Minutes:00}:{time.Seconds:00}";
        countdownText.color = isCritical ? countdownCriticalColor : countdownNormalColor;
        ShowCanvasGroup(countdown_canvasGroup);
    }

    public void HideCountdownUI()
    {
        if (countdown_canvasGroup == null)
            return;

        HideCanvasGroup(countdown_canvasGroup);
    }

    private void ShowCanvasGroup(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideCanvasGroup(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
