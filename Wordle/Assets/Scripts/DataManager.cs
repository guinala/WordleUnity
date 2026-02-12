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

    private void LoadData()
    {
        coins = PlayerPrefs.GetInt("Coins", 150);
        bestScore = PlayerPrefs.GetInt("BestScore");
        score = PlayerPrefs.GetInt("Score");
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
