using System;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager instance;

    [Header("Data")] 
    private int coins;
    private int score;
    private int bestScore;

    [Header("Match Hint Data")]
    private int keyboardHintUses;
    private int letterHintUses;
    private int textHintUses;
    private int betHintUses;
    private int matchHintScorePenalty;
    
    [Header("Events")]
    public static Action OnCoinsChanged;
    public static Action OnHintUsageChanged;

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

    public int GetKeyboardHintUses()
    {
        return keyboardHintUses;
    }

    public int GetLetterHintUses()
    {
        return letterHintUses;
    }

    public int GetTextHintUses()
    {
        return textHintUses;
    }

    public int GetBetHintUses()
    {
        return betHintUses;
    }

    public int GetMatchHintScorePenalty()
    {
        return matchHintScorePenalty;
    }

    public void RegisterKeyboardHintUse(int scorePenalty)
    {
        keyboardHintUses++;
        matchHintScorePenalty += Mathf.Max(0, scorePenalty);
        OnHintUsageChanged?.Invoke();
    }

    public void RegisterLetterHintUse(int scorePenalty)
    {
        letterHintUses++;
        matchHintScorePenalty += Mathf.Max(0, scorePenalty);
        OnHintUsageChanged?.Invoke();
    }

    public void RegisterTextHintUse(int scorePenalty)
    {
        textHintUses++;
        matchHintScorePenalty += Mathf.Max(0, scorePenalty);
        OnHintUsageChanged?.Invoke();
    }

    public void RegisterBetHintUse(int scorePenalty)
    {
        betHintUses++;
        matchHintScorePenalty += Mathf.Max(0, scorePenalty);
        OnHintUsageChanged?.Invoke();
    }

    public void ReduceMatchHintPenalty(int amount)
    {
        matchHintScorePenalty -= Mathf.Max(0, amount);
        matchHintScorePenalty = Mathf.Max(matchHintScorePenalty, 0);
        OnHintUsageChanged?.Invoke();
    }

    public void ResetHintUsageForMatch()
    {
        keyboardHintUses = 0;
        letterHintUses = 0;
        textHintUses = 0;
        betHintUses = 0;
        matchHintScorePenalty = 0;
        OnHintUsageChanged?.Invoke();
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
