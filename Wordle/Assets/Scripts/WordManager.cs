using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordManager : MonoBehaviour
{
    public static WordManager instance;

    private const string DailyChallengeDateKey = "DailyChallengeDate";
    private const string DailyChallengeJsonKey = "DailyChallengeJson";

    [Header("Elements")]
    [SerializeField] private string secretWord;
    [SerializeField] private TextAsset wordsText;
    [SerializeField] private bool generateWithAI;
    private string words;

    [Header("Settings")]
    private bool shouldReset;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        words = wordsText.text;
    }

    private void Start()
    {
        GameManager.OnGameStateChanged += GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback += ClearWords;
        shouldReset = true;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback -= ClearWords;
    }

    private void ClearWords()
    {
        secretWord = "";
        shouldReset = true;
    }

    public string GetSecretWord()
    {
        return secretWord.ToUpper();
    }

    private void SetNewSecretWordFromText()
    {
        int wordCount = (words.Length + 2) / 7;

        int wordIndex = Random.Range(0, wordCount);
        int wordStartIndex = wordIndex * 7;

        secretWord = words.Substring(wordStartIndex, 5).ToUpper();

        APIManager.instance.ClearDailyHint();
        UIManager.Instance.UpdateDailyChallengeBlock(string.Empty, string.Empty, false);
        shouldReset = false;
    }

    private async void SetNewSecretWordFromAI()
    {
        UIManager.Instance.ShowLoading(false);
        string generatedWord = await APIManager.instance.SetAIWord();
        generatedWord = generatedWord.Trim();

        if (string.IsNullOrEmpty(generatedWord) || generatedWord.Length != 5)
        {
            SetNewSecretWordFromText();
            return;
        }

        secretWord = generatedWord.ToUpper();
        APIManager.instance.ClearDailyHint();
        UIManager.Instance.UpdateDailyChallengeBlock(string.Empty, string.Empty, false);
        shouldReset = false;
    }

    private async void SetDailyFirstWord()
    {
        DailyChallengeData challenge;

        challenge = TryGetStoredDailyChallenge();
        if (challenge == null)
            challenge = await TryFetchDailyChallenge();

        if (challenge != null)
        {
            ApplyDailyChallenge(challenge);
            return;
        }

        if (!generateWithAI)
            SetNewSecretWordFromText();
        else
            SetNewSecretWordFromAI();
    }

    private void ApplyDailyChallenge(DailyChallengeData challenge)
    {
        secretWord = challenge.Word.ToUpper();
        APIManager.instance.SetCurrentWordAndHint(challenge.Word, challenge.Hint);
        UIManager.Instance.UpdateDailyChallengeBlock(challenge.Theme, challenge.Rule, true);
        shouldReset = false;
    }

    private DailyChallengeData TryGetStoredDailyChallenge()
    {
        DailyChallengeData challenge = null;
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string savedDate = PlayerPrefs.GetString(DailyChallengeDateKey, string.Empty);

        if (savedDate != today)
            return null;

        string savedJson = PlayerPrefs.GetString(DailyChallengeJsonKey, string.Empty);
        if (!DailyChallengeParser.TryParse(savedJson, out challenge))
            return null;

        string language = PlayerPrefs.GetString("Language", "Spanish");
        return DailyChallengeValidator.IsValid(challenge, language) ? challenge : null;
    }

    private async System.Threading.Tasks.Task<DailyChallengeData> TryFetchDailyChallenge()
    {
        DailyChallengeData challenge = null;
        UIManager.Instance.ShowLoading(false);

        string response = await APIManager.instance.SetDailyChallenge();
        if (!DailyChallengeParser.TryParse(response, out challenge))
            return null;

        string language = PlayerPrefs.GetString("Language", "Spanish");
        if (!DailyChallengeValidator.IsValid(challenge, language))
            return null;

        SaveDailyChallenge(response);
        return challenge;
    }

    private void SaveDailyChallenge(string jsonResponse)
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string cleanJson = DailyChallengeParser.CleanJson(jsonResponse);
        PlayerPrefs.SetString(DailyChallengeDateKey, today);
        PlayerPrefs.SetString(DailyChallengeJsonKey, cleanJson);
        PlayerPrefs.Save();
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Menu:
                break;

            case GameState.Game:
                if (shouldReset)
                    SetDailyFirstWord();
                break;

            case GameState.LevelComplete:
                shouldReset = true;
                break;

            case GameState.GameOver:
                shouldReset = true;
                break;
        }
    }
}
