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
        ShowCanvasGroup(game_canvasGroup);
    }
    
    private void ShowLevelComplete()
    {
        levelCompleteCoins.text = DataManager.instance.GetCoins().ToString();
        levelCompleteScore.text = DataManager.instance.GetScore().ToString();
        levelCompleteBestScore.text = DataManager.instance.GetBestScore().ToString();
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
    
    public void ShowSettings()
    {
        ShowCanvasGroup(settings_canvasGroup);
    }
    
    public void HideSettings()
    {
        HideCanvasGroup(settings_canvasGroup);
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
        ShowCanvasGroup(menu_canvasGroup);
    }
    
    private void HideMenu()
    {
        HideCanvasGroup(menu_canvasGroup);
    }
    
    private void HideGame()
    {
        HideCanvasGroup(game_canvasGroup);
    }
    
    private void HideLevelComplete()
    {
        HideCanvasGroup(levelComplete_canvasGroup);
    }

    private void HideGameOver()
    {
        HideCanvasGroup(gameOver_canvasGroup);
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
