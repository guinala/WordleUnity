using System;
using System.Collections;
using Assets.SimpleLocalization.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;
    
    [Header("Elements")]
    [SerializeField] private WordContainer[] wordContainers;
    [SerializeField] private Button tryButton;
    [SerializeField] private KeyboardColorizer keyboardColorizer;
    [SerializeField] private UIManager uiManager;

    [Header("Settings")] 
    private int currentWordContainerIndex;
    private bool canAddLetter = true;
    private bool shouldReset;

    [Header("Events")] 
    public static Action onLetterAdded;
    public static Action onLetterRemoved;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Initialize();
        Key.OnKeyPressed += KeyPressedCallback;
        GameManager.OnGameStateChanged += GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback += Clear;
    }

    private void OnDestroy()
    {
        Key.OnKeyPressed -= KeyPressedCallback;
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback -= Clear;
    }

    private void Clear()
    {
        Initialize();
        keyboardColorizer.Initialize();
    }
    
    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Game:
                if(shouldReset)
                    Initialize();
                break;
            
            case GameState.LevelComplete:
                shouldReset = true;
                break;
            
            case GameState.GameOver:
                shouldReset = true;
                break;
        }
    }
    
    public void Initialize()
    {
        currentWordContainerIndex = 0;
        canAddLetter = true;

        DataManager.instance.ResetHintUsageForMatch();
        DisableTryButton();
        
        for (int i = 0; i < wordContainers.Length; i++)
            wordContainers[i].Initialize();

        shouldReset = false;
    }

    private void KeyPressedCallback(char letter)
    {
        if (!canAddLetter)
            return;

        if (MatchModifierManager.Instance != null && !MatchModifierManager.Instance.CanUseKey(letter, out string keyFeedback))
        {
            ShowModifierFeedback(keyFeedback);
            return;
        }
        
        wordContainers[currentWordContainerIndex].Add(letter);

        if (wordContainers[currentWordContainerIndex].IsComplete())
        {
            canAddLetter = false;
            EnableTryButton();
            //CheckWord();
        }
        
        onLetterAdded?.Invoke();
    }

    public void CheckWord()
    {
        StartCoroutine(CheckWordRoutine());
    }
    
    private IEnumerator CheckWordRoutine()
    {
        string wordToCheck = wordContainers[currentWordContainerIndex].GetWord();

        if (MatchModifierManager.Instance != null && !MatchModifierManager.Instance.ValidateWordForCurrentTurn(wordToCheck, out string validationFeedback))
        {
            ShowModifierFeedback(validationFeedback);
            canAddLetter = true;
            DisableTryButton();
            yield break;
        }

        string secretWord = WordManager.instance.GetSecretWord();
        
        wordContainers[currentWordContainerIndex].Colorize(secretWord);
        yield return new WaitForSeconds(0.6f);
        keyboardColorizer.Colorize(secretWord, wordToCheck);

        if (wordToCheck == secretWord)
        {
            SetLevelComplete();
        }
        else
        {
            currentWordContainerIndex++;
            DisableTryButton();

            if (MatchModifierManager.Instance != null)
                MatchModifierManager.Instance.ConfigureTurn(currentWordContainerIndex);

            if (currentWordContainerIndex >= wordContainers.Length)
            {
                Debug.Log("Game Over");
                DataManager.instance.ResetScore();
                GameManager.Instance.SetGameState(GameState.GameOver);
            }
            else
            {
                canAddLetter = true;
            }
        }
    }

    private void SetLevelComplete()
    {
        UpdateData();
        GameManager.Instance.SetGameState(GameState.LevelComplete);
    }

    private void UpdateData()
    {
        int baseScoreToAdd = 6 - currentWordContainerIndex;
        int hintPenalty = DataManager.instance.GetMatchHintScorePenalty();
        int scoreToAdd = Mathf.Max(0, baseScoreToAdd - hintPenalty);

        int scoreToAdd = 6 - currentWordContainerIndex;

        if (MatchModifierManager.Instance != null)
        {
            int bonus = MatchModifierManager.Instance.GetEarlyHitBonus(currentWordContainerIndex);
            scoreToAdd += bonus;

            if (bonus > 0)
                ShowModifierFeedback(LocalizationManager.Localize("Gameplay.Modifier.BonusAwarded", bonus));
        }
        
        DataManager.instance.IncreaseScore(scoreToAdd);
        DataManager.instance.AddCoins(scoreToAdd * 3);
    }
    
    public void BackspacePressedCallback()
    {
        if (!GameManager.Instance.IsGameState())
            return;
        bool removedLetter = wordContainers[currentWordContainerIndex].RemoveLetter();
        
        if(removedLetter)
            DisableTryButton();
        canAddLetter = true;
        
        onLetterRemoved?.Invoke();
    }

    private void ShowModifierFeedback(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        if (uiManager != null)
            uiManager.SetModifierMessage(message);
    }

    private void EnableTryButton()
    {
        tryButton.interactable = true;
    }

    private void DisableTryButton()
    {
        tryButton.interactable = false;
    }
    
    public WordContainer GetCurrentWordContainer()
    {
        return wordContainers[currentWordContainerIndex];
    }
}
