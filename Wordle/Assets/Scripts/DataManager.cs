using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PerkType
{
    HintDiscount,
    RevealedLetter,
    ConditionalExtraAttempt
}

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

    private const int BaseXpPerLevel = 100;

    [Header("Data")]
    private int coins;
    private int score;
    private int bestScore;
    private int xp;
    private int level;
    private int wordsSolved;
    private int perfectRounds;
    private int hintsUsed;

    private readonly HashSet<PerkType> unlockedPerks = new HashSet<PerkType>();
    private readonly HashSet<PerkType> equippedPerks = new HashSet<PerkType>();

    [Header("Events")]
    public static Action OnCoinsChanged;
    public static Action OnProgressionChanged;

    [Header("Match Hint Data")]
    private int keyboardHintUses;
    private int letterHintUses;
    private int textHintUses;
    private int betHintUses;
    private int matchHintScorePenalty;
    
    [Header("Events")]
    public static Action OnCoinsChanged;
    public static Action OnHintUsageChanged;

    private const string ChallengeResultsKey = "ChallengeResults";
    private readonly Dictionary<string, ChallengeResultData> challengeResults = new Dictionary<string, ChallengeResultData>();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        LoadData();
    }

    public int GetCoins() => coins;
    public int GetScore() => score;
    public int GetBestScore() => bestScore;
    public int GetXp() => xp;
    public int GetLevel() => level;
    public int GetWordsSolved() => wordsSolved;
    public int GetPerfectRounds() => perfectRounds;
    public int GetHintsUsed() => hintsUsed;
    public int GetXpToNextLevel() => Mathf.Max(0, GetRequiredXpForLevel(level + 1) - xp);

    public bool IsPerkUnlocked(PerkType perkType) => unlockedPerks.Contains(perkType);
    public bool IsPerkEquipped(PerkType perkType) => equippedPerks.Contains(perkType);

    public string GetPerkStateText()
    {
        string[] rows = Enum.GetNames(typeof(PerkType))
            .Select(perkName =>
            {
                PerkType perk = (PerkType)Enum.Parse(typeof(PerkType), perkName);
                string unlockedState = IsPerkUnlocked(perk) ? "Desbloqueado" : "Bloqueado";
                string equippedState = IsPerkEquipped(perk) ? "Equipado" : "No equipado";
                return $"{perkName}: {unlockedState} - {equippedState}";
            })
            .ToArray();

        return string.Join("\n", rows);
    }

    public string GetPerkUnlockRulesText()
    {
        return "Reglas de desbloqueo:\n" +
               "- HintDiscount: nivel 2 o usar 5 pistas.\n" +
               "- RevealedLetter: completar 3 palabras.\n" +
               "- ConditionalExtraAttempt: completar 2 rondas perfectas o mejor puntaje >= 12.";
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
        coins = Mathf.Max(coins - amount, 0);
        SaveData();

        OnCoinsChanged?.Invoke();
    }

    public void IncreaseScore(int amount)
    {
        score += amount;

        if (score > bestScore)
            bestScore = score;

        SaveData();
        EvaluatePerkUnlocks();
    }

    public void ResetScore()
    {
        score = 0;
        SaveData();
        OnProgressionChanged?.Invoke();
    }

    public void AddXp(int amount)
    {
        xp += Mathf.Max(0, amount);

        while (xp >= GetRequiredXpForLevel(level + 1))
            level++;

        SaveData();
        EvaluatePerkUnlocks();
    }

    public void RegisterLevelComplete(bool perfectRound)
    {
        wordsSolved++;
        if (perfectRound)
            perfectRounds++;

        SaveData();
        EvaluatePerkUnlocks();
    }

    public void RegisterHintUsed()
    {
        hintsUsed++;
        SaveData();
        EvaluatePerkUnlocks();
    }

    public bool TryEquipPerk(PerkType perkType)
    {
        if (!IsPerkUnlocked(perkType))
            return false;

        equippedPerks.Add(perkType);
        SaveData();
        OnProgressionChanged?.Invoke();
        return true;
    }

    private void EvaluatePerkUnlocks()
    {
        UnlockPerkIfNeeded(PerkType.HintDiscount, level >= 2 || hintsUsed >= 5);
        UnlockPerkIfNeeded(PerkType.RevealedLetter, wordsSolved >= 3);
        UnlockPerkIfNeeded(PerkType.ConditionalExtraAttempt, perfectRounds >= 2 || bestScore >= 12);

        SaveData();
        OnProgressionChanged?.Invoke();
    }

    private void UnlockPerkIfNeeded(PerkType perkType, bool condition)
    {
        if (!condition || unlockedPerks.Contains(perkType))
            return;

        unlockedPerks.Add(perkType);
        equippedPerks.Add(perkType);
    }

    private int GetRequiredXpForLevel(int targetLevel)
    {
        return (targetLevel - 1) * BaseXpPerLevel;
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
        xp = PlayerPrefs.GetInt("XP");
        level = Mathf.Max(1, PlayerPrefs.GetInt("Level", 1));
        wordsSolved = PlayerPrefs.GetInt("WordsSolved");
        perfectRounds = PlayerPrefs.GetInt("PerfectRounds");
        hintsUsed = PlayerPrefs.GetInt("HintsUsed");

        DeserializePerks(PlayerPrefs.GetString("UnlockedPerks", string.Empty), unlockedPerks);
        DeserializePerks(PlayerPrefs.GetString("EquippedPerks", string.Empty), equippedPerks);

        foreach (PerkType perk in unlockedPerks)
            equippedPerks.Add(perk);

        EvaluatePerkUnlocks();
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
        PlayerPrefs.SetInt("XP", xp);
        PlayerPrefs.SetInt("Level", level);
        PlayerPrefs.SetInt("WordsSolved", wordsSolved);
        PlayerPrefs.SetInt("PerfectRounds", perfectRounds);
        PlayerPrefs.SetInt("HintsUsed", hintsUsed);
        PlayerPrefs.SetString("UnlockedPerks", SerializePerks(unlockedPerks));
        PlayerPrefs.SetString("EquippedPerks", SerializePerks(equippedPerks));
    }

    private string SerializePerks(HashSet<PerkType> perkSet)
    {
        return string.Join(",", perkSet.Select(x => x.ToString()));
    }

    private void DeserializePerks(string serializedPerks, HashSet<PerkType> target)
    {
        target.Clear();

        if (string.IsNullOrEmpty(serializedPerks))
            return;

        string[] values = serializedPerks.Split(',');
        for (int i = 0; i < values.Length; i++)
        {
            if (Enum.TryParse(values[i], out PerkType perk))
                target.Add(perk);
        }
    }
}
