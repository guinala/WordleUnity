using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class WordManager : MonoBehaviour
{
    public static WordManager instance;

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
        Debug.Log("Patatuelas con atun y tomate");
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
                    if(!generateWithAI)
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
