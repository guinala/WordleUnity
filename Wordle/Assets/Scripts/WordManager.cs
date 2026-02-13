using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordManager : MonoBehaviour
{
    [Serializable]
    private class MutatorsData
    {
        public bool generateWithAI;
    }

    [Serializable]
    private class ChallengeSeedData
    {
        public string word;
        public string language;
        public MutatorsData mutators;
    }

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
    private string currentChallengeCode;
    private ChallengeSeedData importedSeed;
    

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
        currentChallengeCode = string.Empty;
        importedSeed = null;
        shouldReset = true;
    }

    public string GetSecretWord()
    {
        return secretWord.ToUpper();
    }

    public string GetCurrentChallengeCode()
    {
        if (string.IsNullOrEmpty(currentChallengeCode) && !string.IsNullOrEmpty(secretWord))
            currentChallengeCode = BuildChallengeCode(secretWord.ToUpper());

        return currentChallengeCode;
    }

    public bool HasImportedSeed()
    {
        return importedSeed != null;
    }

    public bool TryLoadSeedFromCode(string code, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(code))
        {
            errorMessage = "El código está vacío.";
            return false;
        }

        try
        {
            string json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(code.Trim()));
            ChallengeSeedData parsedData = JsonUtility.FromJson<ChallengeSeedData>(json);

            if (parsedData == null || string.IsNullOrEmpty(parsedData.word) || parsedData.word.Trim().Length != 5)
            {
                errorMessage = "Código inválido.";
                return false;
            }

            parsedData.word = parsedData.word.Trim().ToUpper();
            if (parsedData.mutators == null)
                parsedData.mutators = new MutatorsData();

            importedSeed = parsedData;
            secretWord = parsedData.word;
            currentChallengeCode = BuildChallengeCode(parsedData.word);

            ApplySeedSettings(parsedData);
            shouldReset = false;
            return true;
        }
        catch (Exception)
        {
            errorMessage = "No se pudo leer el código.";
            return false;
        }
    }

    private string BuildChallengeCode(string word)
    {
        ChallengeSeedData seedData = new ChallengeSeedData
        {
            word = word.ToUpper(),
            language = PlayerPrefs.GetString("Language", "Spanish"),
            mutators = new MutatorsData
            {
                generateWithAI = generateWithAI
            }
        };

        string json = JsonUtility.ToJson(seedData);
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    }

    private void ApplySeedSettings(ChallengeSeedData seedData)
    {
        if (!string.IsNullOrEmpty(seedData.language))
        {
            PlayerPrefs.SetString("Language", seedData.language);
            if (MultiLanguage.Instance != null)
                MultiLanguage.Instance.Language(seedData.language);
        }

        generateWithAI = seedData.mutators.generateWithAI;
    }

    private void SetNewSecretWordFromText()
    {
        int wordCount = (words.Length + 2) / 7;

        int wordIndex = Random.Range(0, wordCount);
        int wordStartIndex = wordIndex * 7;

        secretWord = words.Substring(wordStartIndex, 5).ToUpper();
        currentChallengeCode = BuildChallengeCode(secretWord);

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
        currentChallengeCode = BuildChallengeCode(secretWord);
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
                {
                    if (importedSeed != null)
                    {
                        secretWord = importedSeed.word;
                        currentChallengeCode = BuildChallengeCode(secretWord);
                        ApplySeedSettings(importedSeed);
                        importedSeed = null;
                        shouldReset = false;
                    }
                    else if(!generateWithAI)
                        SetNewSecretWordFromText();
                    else
                    {
                        SetNewSecretWordFromAI();
                    }
                }
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
