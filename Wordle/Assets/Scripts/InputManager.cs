using System;
using System.Collections;
using System.Collections.Generic;
using Assets.SimpleLocalization.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class InputManager : MonoBehaviour
{
    private enum TimerMode
    {
        PerAttempt,
        PerMatch
    }

    private enum TimeoutBehavior
    {
        AutoSubmit,
        PenaltyAndSkipAttempt
    }

    public static InputManager instance;

    [Header("Elements")] [SerializeField] private WordContainer[] wordContainers;
    [SerializeField] private Button tryButton;
    [SerializeField] private KeyboardColorizer keyboardColorizer;
    [SerializeField] private UIManager uiManager;

    [Header("Settings")] private int currentWordContainerIndex;
    private bool canAddLetter = true;
    private bool shouldReset;

    [Header("Timer")] [SerializeField] private TimerMode timerMode = TimerMode.PerAttempt;
    [SerializeField] private TimeoutBehavior timeoutBehavior = TimeoutBehavior.AutoSubmit;
    [SerializeField] private float perAttemptDuration = 30f;
    [SerializeField] private float perMatchDuration = 180f;
    [SerializeField] private float criticalThreshold = 5f;
    [SerializeField] private int timeoutScorePenalty = 1;

    private float remainingTime;
    private float currentAttemptElapsed;
    private float elapsedMatchTime;
    private bool timerActive;
    private bool wordCheckInProgress;

    [Header("Events")] [Header("Events")] public static Action onLetterAdded;
    public static Action onLetterRemoved;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
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

    private void Update()
    {
        if (!timerActive || !GameManager.Instance.IsGameState())
            return;

        float deltaTime = Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime - deltaTime);
        currentAttemptElapsed += deltaTime;

        if (timerMode == TimerMode.PerMatch)
            elapsedMatchTime += deltaTime;

        UIManager.Instance?.UpdateCountdownUI(remainingTime, remainingTime <= criticalThreshold);

        if (remainingTime <= 0f)
            HandleTimeExpired();
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
                if (shouldReset)
                    Initialize();
                break;

            case GameState.LevelComplete:
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

        StartTimer();

        shouldReset = false;
    }

    private void KeyPressedCallback(char letter)
    {
        if (!canAddLetter)
            return;

        if (MatchModifierManager.Instance != null &&
            !MatchModifierManager.Instance.CanUseKey(letter, out string keyFeedback))
        {
            ShowModifierFeedback(keyFeedback);
            return;
        }

        wordContainers[currentWordContainerIndex].Add(letter);

        if (wordContainers[currentWordContainerIndex].IsComplete())
        {
            canAddLetter = false;
            EnableTryButton();
        }

        onLetterAdded?.Invoke();
    }

    public void CheckWord()
    {
        if (wordCheckInProgress)
            return;

        StartCoroutine(CheckWordRoutine());
    }

    private IEnumerator CheckWordRoutine()
    {
        wordCheckInProgress = true;

        string wordToCheck = wordContainers[currentWordContainerIndex].GetWord();

        if (MatchModifierManager.Instance != null &&
            !MatchModifierManager.Instance.ValidateWordForCurrentTurn(wordToCheck, out string validationFeedback))
        {
            ShowModifierFeedback(validationFeedback);
            canAddLetter = true;
            DisableTryButton();
            yield break;
        }

        string secretWord = WordManager.instance.GetSecretWord();

        DataManager.instance.RegisterAttemptTime(currentAttemptElapsed);

        wordContainers[currentWordContainerIndex].Colorize(secretWord);
        float checkDelay = EnvironmentState.Instance.GetWordCheckDelay(0.6f);
        yield return new WaitForSeconds(checkDelay);
        keyboardColorizer.Colorize(secretWord, wordToCheck);

        if (wordToCheck == secretWord)
        {
            SetLevelComplete();
        }
        else
        {
            GoToNextAttempt();
            currentWordContainerIndex++;
            DisableTryButton();

            if (MatchModifierManager.Instance != null)
                MatchModifierManager.Instance.ConfigureTurn(currentWordContainerIndex);

            if (currentWordContainerIndex >= wordContainers.Length)
            {
                Debug.Log("Game Over");
                string challengeCode = WordManager.instance.GetCurrentChallengeCode();
                DataManager.instance.SaveChallengeResult(challengeCode, false, 0, 0);
                DataManager.instance.ResetScore();
                GameManager.Instance.SetGameState(GameState.GameOver);
                int exactLetters = GetExactLettersCount(wordToCheck, secretWord);
                if (PerkManager.Instance == null)
                    Debug.LogWarning("PerkManager.Instance is null. Conditional extra attempt perk cannot be evaluated.");

                bool extraAttemptGranted = PerkManager.Instance != null &&
                                           PerkManager.Instance.CanGrantConditionalExtraAttempt(exactLetters);

                if (extraAttemptGranted)
                {
                    currentWordContainerIndex = wordContainers.Length - 1;
                    wordContainers[currentWordContainerIndex].Initialize();
                    canAddLetter = true;
                }
                else
                {
                    Debug.Log("Game Over");
                    DataManager.instance.ResetScore();
                    GameManager.Instance.SetGameState(GameState.GameOver);
                }
            }
            else
            {
                canAddLetter = true;
            }
        }

        wordCheckInProgress = false;
    }

    private void SetLevelComplete()
    {
        timerActive = false;
        UIManager.Instance?.HideCountdownUI();

        float completionTime = timerMode == TimerMode.PerMatch ? elapsedMatchTime : currentAttemptElapsed;
        DataManager.instance.RegisterMatchTime(completionTime);
        string challengeCode = WordManager.instance.GetCurrentChallengeCode();
        int currentAttempt = currentWordContainerIndex + 1;
        int scoreToAdd = 6 - currentWordContainerIndex;

        DataManager.instance.SaveChallengeResult(challengeCode, true, scoreToAdd, currentAttempt);
        UpdateData();
        GameManager.Instance.SetGameState(GameState.LevelComplete);
    }

    private void UpdateData()
    {
        int baseScoreToAdd = 6 - currentWordContainerIndex;
        float speedFactor = 1f + GetCurrentSpeedFactor();
        int scoreToAdd = Mathf.Max(1, Mathf.RoundToInt(baseScoreToAdd * speedFactor));
        int hintPenalty = DataManager.instance.GetMatchHintScorePenalty();
        
        scoreToAdd = Mathf.Max(0, scoreToAdd - hintPenalty);
        scoreToAdd += EnvironmentState.Instance.GetScoreBonus();

        if (MatchModifierManager.Instance != null)
        {
            int bonus = MatchModifierManager.Instance.GetEarlyHitBonus(currentWordContainerIndex);
            scoreToAdd += bonus;

            if (bonus > 0)
                ShowModifierFeedback(LocalizationManager.Localize("Gameplay.Modifier.BonusAwarded", bonus));
        }

        DataManager.instance.IncreaseScore(scoreToAdd);
        DataManager.instance.AddCoins(scoreToAdd * 3);
        DataManager.instance.AddXp(scoreToAdd * 20);
        DataManager.instance.RegisterLevelComplete(currentWordContainerIndex == 0);
    }

    private float GetCurrentSpeedFactor()
    {
        float totalTime = timerMode == TimerMode.PerMatch ? perMatchDuration : perAttemptDuration;

        if (totalTime <= 0f)
            return 0f;

        return Mathf.Clamp01(remainingTime / totalTime);
    }

    private void HandleTimeExpired()
    {
        if (wordCheckInProgress)
            return;

        switch (timeoutBehavior)
        {
            case TimeoutBehavior.AutoSubmit:
                if (timerMode == TimerMode.PerAttempt)
                    CheckWord();
                else
                    TriggerGameOver();
                break;

            case TimeoutBehavior.PenaltyAndSkipAttempt:
                DataManager.instance.IncreaseScore(-Mathf.Abs(timeoutScorePenalty));

                if (timerMode == TimerMode.PerMatch)
                    TriggerGameOver();
                else
                    GoToNextAttempt();
                break;
        }
    }

    private void GoToNextAttempt()
    {
        currentWordContainerIndex++;
        DisableTryButton();

        if (currentWordContainerIndex >= wordContainers.Length)
        {
            TriggerGameOver();
            return;
        }

        canAddLetter = true;
        currentAttemptElapsed = 0f;

        if (timerMode == TimerMode.PerAttempt)
            remainingTime = perAttemptDuration;
    }

    private void TriggerGameOver()
    {
        timerActive = false;
        UIManager.Instance?.HideCountdownUI();
        DataManager.instance.ResetScore();
        GameManager.Instance.SetGameState(GameState.GameOver);
    }

    private void StartTimer()
    {
        timerActive = true;
        wordCheckInProgress = false;
        currentAttemptElapsed = 0f;
        elapsedMatchTime = 0f;

        remainingTime = timerMode == TimerMode.PerMatch ? perMatchDuration : perAttemptDuration;
        UIManager.Instance?.UpdateCountdownUI(remainingTime, remainingTime <= criticalThreshold);
    }

    public void BackspacePressedCallback()
    {
        if (!GameManager.Instance.IsGameState())
            return;
        bool removedLetter = wordContainers[currentWordContainerIndex].RemoveLetter();

        if (removedLetter)
            DisableTryButton();
        canAddLetter = true;

        onLetterRemoved?.Invoke();
    }

    public void RevealPerkLetter()
    {
        if (PerkManager.Instance == null)
        {
            Debug.LogWarning("PerkManager.Instance is null. RevealPerkLetter was ignored.");
            return;
        }

        if (wordContainers == null || wordContainers.Length == 0)
            return;

        string secretWord = WordManager.instance.GetSecretWord();
        if (string.IsNullOrEmpty(secretWord))
            return;

        List<int> possibleIndices = new List<int>();
        for (int i = 0; i < secretWord.Length; i++)
            possibleIndices.Add(i);

        int selectedIndex = possibleIndices[Random.Range(0, possibleIndices.Count)];
        wordContainers[0].AddAsHint(selectedIndex, secretWord[selectedIndex]);
    }

    private int GetExactLettersCount(string wordToCheck, string secretWord)
    {
        int exactLetters = 0;
        int length = Mathf.Min(wordToCheck.Length, secretWord.Length);

        for (int i = 0; i < length; i++)
        {
            if (wordToCheck[i] == secretWord[i])
                exactLetters++;
        }

        return exactLetters;
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

    public int GetCurrentAttemptNumber()
    {
        return currentWordContainerIndex + 1;
    }
}
