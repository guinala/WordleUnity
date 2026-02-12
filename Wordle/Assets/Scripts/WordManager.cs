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
        // if(!generateWithAI)
        //     SetNewSecretWordFromText();
        // else
        // {
        //     SetNewSecretWordFromAI();
        // }
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

        shouldReset = false;
    }

    private async void SetNewSecretWordFromAI()
    {
        UIManager.Instance.ShowLoading(false);
        string word = await APIManager.instance.SetAIWord();
        word = word.Trim();
        Debug.Log(word);
        if(String.IsNullOrEmpty(word) || word.Length != 5)
        {
            Debug.Log("No vale la pena");
            SetNewSecretWordFromText();
            return;
        }
        
        secretWord = word.ToUpper();
        currentChallengeCode = BuildChallengeCode(secretWord);
        shouldReset = false;
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        Debug.Log("Cambio de estado en WordManager: " + gameState.ToString());
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
