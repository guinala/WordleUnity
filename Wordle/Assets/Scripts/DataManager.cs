using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("Data")] 
    private int coins;
    private int score;
    private int bestScore;
    private float bestAttemptTime = float.MaxValue;
    private float bestMatchTime = float.MaxValue;
    
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

    public float GetBestAttemptTime()
    {
        return bestAttemptTime;
    }

    public float GetBestMatchTime()
    {
        return bestMatchTime;
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
        score = Mathf.Max(score, 0);

        if (score > bestScore)
        {
            bestScore = score;
        }
        SaveData();
    }

    public void RegisterAttemptTime(float seconds)
    {
        if (seconds <= 0f)
            return;

        bestAttemptTime = Mathf.Min(bestAttemptTime, seconds);
        SaveData();
    }

    public void RegisterMatchTime(float seconds)
    {
        if (seconds <= 0f)
            return;

        bestMatchTime = Mathf.Min(bestMatchTime, seconds);
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
        bestAttemptTime = PlayerPrefs.GetFloat("BestAttemptTime", float.MaxValue);
        bestMatchTime = PlayerPrefs.GetFloat("BestMatchTime", float.MaxValue);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.SetFloat("BestAttemptTime", bestAttemptTime);
        PlayerPrefs.SetFloat("BestMatchTime", bestMatchTime);
    }
}
