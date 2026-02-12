using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    
    [Header("Elements")]
    [SerializeField] private WordContainer[] wordContainers;
    [SerializeField] private Button tryButton;
    [SerializeField] private KeyboardColorizer keyboardColorizer;

    [Header("Settings")] 
    private int currentWordContainerIndex;
    private bool canAddLetter = true;
    private bool shouldReset;

    [Header("Timer")]
    [SerializeField] private TimerMode timerMode = TimerMode.PerAttempt;
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
        if (wordCheckInProgress)
            return;

        StartCoroutine(CheckWordRoutine());
    }
    
    private IEnumerator CheckWordRoutine()
    {
        wordCheckInProgress = true;

        string wordToCheck = wordContainers[currentWordContainerIndex].GetWord();
        string secretWord = WordManager.instance.GetSecretWord();

        DataManager.instance.RegisterAttemptTime(currentAttemptElapsed);
        
        wordContainers[currentWordContainerIndex].Colorize(secretWord);
        yield return new WaitForSeconds(0.6f);
        keyboardColorizer.Colorize(secretWord, wordToCheck);

        if (wordToCheck == secretWord)
        {
            SetLevelComplete();
        }
        else
        {
            GoToNextAttempt();
        }

        wordCheckInProgress = false;
    }

    private void SetLevelComplete()
    {
        timerActive = false;
        UIManager.Instance?.HideCountdownUI();

        float completionTime = timerMode == TimerMode.PerMatch ? elapsedMatchTime : currentAttemptElapsed;
        DataManager.instance.RegisterMatchTime(completionTime);
        UpdateData();
        GameManager.Instance.SetGameState(GameState.LevelComplete);
    }

    private void UpdateData()
    {
        int baseScore = 6 - currentWordContainerIndex;
        float speedFactor = 1f + GetCurrentSpeedFactor();
        int scoreToAdd = Mathf.Max(1, Mathf.RoundToInt(baseScore * speedFactor));
        
        DataManager.instance.IncreaseScore(scoreToAdd);
        DataManager.instance.AddCoins(scoreToAdd * 3);
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
        
        if(removedLetter)
            DisableTryButton();
        canAddLetter = true;
        
        onLetterRemoved?.Invoke();
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
