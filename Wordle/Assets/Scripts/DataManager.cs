using System;
using UnityEditor.Overlays;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("Data")] 
    private int coins;
    private int score;
    private int bestScore;
    
    [Header("Events")]
    public static Action OnCoinsChanged;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
        
        LoadData();
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetScore()
    {
        return score;
    }
    
    public int GetBestScore()
    {
        return bestScore;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveData();
        
        OnCoinsChanged?.Invoke();
    }

    public void RemoveCoins(int amount)
    {
        coins -= amount;
        coins = Mathf.Max(coins, 0);
        SaveData();
        
        OnCoinsChanged?.Invoke();
    }

    public void IncreaseScore(int amount)
    {
        score += amount;

        if (score > bestScore)
        {
            bestScore = score;
        }
        SaveData();
    }

    public void ResetScore()
    {
        score = 0;
        SaveData();
    }

    private void LoadData()
    {
        coins = PlayerPrefs.GetInt("Coins", 150);
        bestScore = PlayerPrefs.GetInt("BestScore");
        score = PlayerPrefs.GetInt("Score");
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetInt("Score", score);
    }
}
