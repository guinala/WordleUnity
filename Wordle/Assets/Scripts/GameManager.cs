using System;
using UnityEngine;

public enum GameState
{
    Menu,
    Game,
    LevelComplete,
    GameOver,
    Idle
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Settings")]
    private GameState _gameState;
    
    [Header("Events")]
    public static Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
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
        SetGameState(GameState.Menu);
    }

    public bool IsGameState()
    {
        return _gameState == GameState.Game;
    }

}
