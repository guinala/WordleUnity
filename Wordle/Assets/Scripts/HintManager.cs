using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class HintManager : MonoBehaviour
{
    [Header("Elements")] 
    [SerializeField] private GameObject keyboard;
    private Key[] keys;
    
    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI keyboardPriceText;
    [SerializeField] private TextMeshProUGUI letterPriceText;
    [SerializeField] private TextMeshProUGUI textHintPriceText;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Settings")]
    [SerializeField] private int keyboardHintPrice;
    [SerializeField] private int letterHintPrice;
    [SerializeField] private int textHintPrice;
    [SerializeField] private bool textHintGiven;
    
    private bool shouldReset;

    private void Awake()
    {
        keys = keyboard.GetComponentsInChildren<Key>();
    }

    private void Start()
    {
        keyboardPriceText.text = keyboardHintPrice.ToString();
        letterPriceText.text = letterHintPrice.ToString();
        textHintPriceText.text = textHintPrice.ToString();
        GameManager.OnGameStateChanged += GameStateChangedCallback;
    }
    
    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
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
                    textHintGiven = false;
                    hintText.text = "";
                    letterHintGivenIndices.Clear();
                    shouldReset = false;
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

    public void KeyboardHint()
    {
        if (DataManager.instance.GetCoins() < keyboardHintPrice)
            return;
        
        string secretWord = WordManager.instance.GetSecretWord();

        List<Key> untouchedKeys = new List<Key>();

        for (int i = 0; i < keys.Length; i++)
        {
            if(keys[i].IsUntouched())
                untouchedKeys.Add(keys[i]);
        }

        List<Key> t_untouchedKeys = new List<Key>(untouchedKeys);
        
        for(int i = 0; i < untouchedKeys.Count; i++)
        {
            if(secretWord.Contains(untouchedKeys[i].GetLetter()))
                t_untouchedKeys.Remove(untouchedKeys[i]);
        }

        if (t_untouchedKeys.Count <= 0)
            return;
        
        int randomIndex = Random.Range(0, t_untouchedKeys.Count);
        t_untouchedKeys[randomIndex].SetInvalid();
        
        DataManager.instance.RemoveCoins(keyboardHintPrice);
    }
    
    List<int> letterHintGivenIndices = new List<int>();

    public void LetterHint()
    {
        if (DataManager.instance.GetCoins() < letterHintPrice)
            return;
        
        if (letterHintGivenIndices.Count >= 5)
        {
            Debug.Log("All hints given");
            return;
        }

        List<int> letterHintNotGivenIndices = new List<int>();
        
        for(int i = 0; i < 5; i++)
        {
            if (!letterHintGivenIndices.Contains(i))
                letterHintNotGivenIndices.Add(i);
        }
        
        WordContainer currentWordContainer = InputManager.instance.GetCurrentWordContainer();
        string secretWord = WordManager.instance.GetSecretWord();
        
        int randomIndex = letterHintNotGivenIndices[Random.Range(0, letterHintNotGivenIndices.Count)];
        letterHintGivenIndices.Add(randomIndex);
        
        currentWordContainer.AddAsHint(randomIndex, secretWord[randomIndex]);
        
        DataManager.instance.RemoveCoins(letterHintPrice);
    }

    public void ShowHintPanel()
    {
        UIManager.Instance.ShowHintUI();
        Debug.Log("Actualmente la variable está en : " + textHintGiven);
        if (!textHintGiven)
        {
            UIManager.Instance.ShowMainHintPanel();
        }
        else
        {
            UIManager.Instance.ShowGivenHintPanel();
        }
    }
    
    public async void TextHint()
    {
        if (DataManager.instance.GetCoins() < textHintPrice)
            return;
        
        if (textHintGiven)
        {
            return;
        }

        UIManager.Instance.HideMainHintPanel();
        UIManager.Instance.ShowLoading(true);
        string hint = await APIManager.instance.SetAIHint();
        hint = hint.Trim();
        
        UIManager.Instance.ShowGivenHintPanel();
        hintText.text = hint;
        
        textHintGiven = true;
    }
}
