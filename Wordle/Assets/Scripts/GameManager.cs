using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Game,
    LevelComplete,
    GameOver,
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Settings")]
    private GameState _gameState;
    
    [Header("Events")]
    public static Action<GameState> OnGameStateChanged;
    public static Action OnGameBackButtonCallback;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (FindObjectOfType<MatchModifierManager>() == null)
                gameObject.AddComponent<MatchModifierManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameState(GameState gameState)
    {
        this._gameState = gameState;
        OnGameStateChanged?.Invoke(this._gameState);
    }

    public void NextButtonCallback()
    {
        SetGameState(GameState.Game);
    }
    
    public void PlayButtonCallback()
    {
        SetGameState(GameState.Game);
    }
    
    public void BackButtonCallback()
    {
        Debug.Log("estado cambiado");
        OnGameBackButtonCallback?.Invoke();
        SetGameState(GameState.Menu);
    }

    public bool IsGameState()
    {
        return _gameState == GameState.Game;
    }

}
