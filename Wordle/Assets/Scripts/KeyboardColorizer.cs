using System;
using UnityEngine;

public class KeyboardColorizer : MonoBehaviour
{
    [Header("Elements")]
    private Key[] _keys;

    [Header("Settings")] 
    private bool shouldReset;

    private void Awake()
    {
        _keys = GetComponentsInChildren<Key>();
    }

    private void Start()
    {
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
        for(int i = 0; i < _keys.Length; i++)
        {
            _keys[i].Initialize();
        }

        shouldReset = false;
    }

    public void Colorize(string secretWord, string wordToCheck)
    {
        for(int i = 0; i < _keys.Length; i++)
        {
            char keyLetter = _keys[i].GetLetter();
            
            for(int j = 0; j < wordToCheck.Length; j++)
            {
                if (keyLetter != wordToCheck[j])
                    continue;
                
                //The key letter we've pressed is equals to the current wordToCheck letter

                if (keyLetter == secretWord[j])
                {
                    //valid
                    _keys[i].SetValid();
                }
                else if (secretWord.Contains(keyLetter))
                {
                    //Potential
                    _keys[i].SetPotential();
                }
                else
                {
                    //Invalid
                    _keys[i].SetInvalid();
                }
            }
        }
    }
}
