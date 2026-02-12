using System;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Serializable]
    private class ChallengeResultData
    {
        public string code;
        public int bestScore;
        public int bestAttempt;
        public int gamesPlayed;
        public int wins;
    }

    [Serializable]
    private class ChallengeResultCollection
    {
        public List<ChallengeResultData> items = new List<ChallengeResultData>();
    }

    public static DataManager instance;

    [Header("Data")] 
    private int coins;
    private int score;
    private int bestScore;
    
    [Header("Events")]
    public static Action OnCoinsChanged;

    private const string ChallengeResultsKey = "ChallengeResults";
    private readonly Dictionary<string, ChallengeResultData> challengeResults = new Dictionary<string, ChallengeResultData>();

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


    public void SaveChallengeResult(string code, bool won, int scoreValue, int attempt)
    {
        if (string.IsNullOrEmpty(code))
            return;

        if (!challengeResults.TryGetValue(code, out ChallengeResultData resultData))
        {
            resultData = new ChallengeResultData
            {
                code = code,
                bestScore = 0,
                bestAttempt = 0,
                gamesPlayed = 0,
                wins = 0
            };
            challengeResults[code] = resultData;
        }

        resultData.gamesPlayed++;

        if (won)
        {
            resultData.wins++;
            if (scoreValue > resultData.bestScore)
                resultData.bestScore = scoreValue;

            if (attempt > 0 && (resultData.bestAttempt == 0 || attempt < resultData.bestAttempt))
                resultData.bestAttempt = attempt;
        }

        SaveData();
    }

    public int GetChallengeBestScore(string code)
    {
        if (string.IsNullOrEmpty(code) || !challengeResults.TryGetValue(code, out ChallengeResultData resultData))
            return 0;

        return resultData.bestScore;
    }

    public int GetChallengeBestAttempt(string code)
    {
        if (string.IsNullOrEmpty(code) || !challengeResults.TryGetValue(code, out ChallengeResultData resultData))
            return 0;

        return resultData.bestAttempt;
    }

    public int GetChallengeGamesPlayed(string code)
    {
        if (string.IsNullOrEmpty(code) || !challengeResults.TryGetValue(code, out ChallengeResultData resultData))
            return 0;

        return resultData.gamesPlayed;
    }

    public int GetChallengeWins(string code)
    {
        if (string.IsNullOrEmpty(code) || !challengeResults.TryGetValue(code, out ChallengeResultData resultData))
            return 0;

        return resultData.wins;
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

        challengeResults.Clear();
        string json = PlayerPrefs.GetString(ChallengeResultsKey, string.Empty);
        if (string.IsNullOrEmpty(json))
            return;

        ChallengeResultCollection collection = JsonUtility.FromJson<ChallengeResultCollection>(json);
        if (collection == null || collection.items == null)
            return;

        for (int i = 0; i < collection.items.Count; i++)
        {
            ChallengeResultData item = collection.items[i];
            if (item == null || string.IsNullOrEmpty(item.code))
                continue;

            challengeResults[item.code] = item;
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetInt("Score", score);

        ChallengeResultCollection collection = new ChallengeResultCollection();
        foreach (ChallengeResultData value in challengeResults.Values)
            collection.items.Add(value);

        PlayerPrefs.SetString(ChallengeResultsKey, JsonUtility.ToJson(collection));
    }
}
