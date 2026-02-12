using System;
using System.Collections.Generic;
using System.Text;
using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public enum MatchModifierType
{
    None,
    TemporaryKeyLock,
    EarlyHitBonus,
    TurnLetterRestriction
}

public enum TurnLetterRestriction
{
    None,
    VowelsOnly,
    ConsonantsOnly
}

public class MatchModifierManager : MonoBehaviour
{
    public static MatchModifierManager Instance;

    private const string Vowels = "AEIOU";
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    [Header("Settings")]
    [SerializeField] private int lockedKeyCount = 3;

    private readonly HashSet<char> _lockedLetters = new HashSet<char>();
    private MatchModifierType _activeModifier = MatchModifierType.None;
    private TurnLetterRestriction _turnRestriction = TurnLetterRestriction.None;

    [Header("Events")]
    public static Action OnActiveModifierChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += GameStateChangedCallback;
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
    }

    private void GameStateChangedCallback(GameState state)
    {
        if (state == GameState.Game)
            InitializeModifier();
    }

    public void InitializeModifier()
    {
        _activeModifier = (MatchModifierType)UnityEngine.Random.Range(1, 4);
        _lockedLetters.Clear();
        _turnRestriction = TurnLetterRestriction.None;

        ConfigureTurn(0);
        OnActiveModifierChanged?.Invoke();
    }

    public void ConfigureTurn(int turnIndex)
    {
        if (_activeModifier == MatchModifierType.TemporaryKeyLock)
            RollLockedLetters();

        if (_activeModifier == MatchModifierType.TurnLetterRestriction)
            _turnRestriction = turnIndex % 2 == 0 ? TurnLetterRestriction.VowelsOnly : TurnLetterRestriction.ConsonantsOnly;
        else
            _turnRestriction = TurnLetterRestriction.None;

        OnActiveModifierChanged?.Invoke();
    }

    public bool CanUseKey(char letter, out string feedback)
    {
        char normalizedLetter = char.ToUpperInvariant(letter);

        if (_activeModifier == MatchModifierType.TemporaryKeyLock && _lockedLetters.Contains(normalizedLetter))
        {
            feedback = LocalizationManager.Localize("Gameplay.Modifier.LockedKeyBlocked", normalizedLetter);
            return false;
        }

        if (_activeModifier == MatchModifierType.TurnLetterRestriction)
        {
            bool isVowel = IsVowel(normalizedLetter);

            if (_turnRestriction == TurnLetterRestriction.VowelsOnly && !isVowel)
            {
                feedback = LocalizationManager.Localize("Gameplay.Modifier.OnlyVowels");
                return false;
            }

            if (_turnRestriction == TurnLetterRestriction.ConsonantsOnly && isVowel)
            {
                feedback = LocalizationManager.Localize("Gameplay.Modifier.OnlyConsonants");
                return false;
            }
        }

        feedback = string.Empty;
        return true;
    }

    public bool ValidateWordForCurrentTurn(string wordToCheck, out string feedback)
    {
        if (_activeModifier != MatchModifierType.TurnLetterRestriction)
        {
            feedback = string.Empty;
            return true;
        }

        for (int i = 0; i < wordToCheck.Length; i++)
        {
            bool isVowel = IsVowel(char.ToUpperInvariant(wordToCheck[i]));
            if (_turnRestriction == TurnLetterRestriction.VowelsOnly && !isVowel)
            {
                feedback = LocalizationManager.Localize("Gameplay.Modifier.OnlyVowels");
                return false;
            }

            if (_turnRestriction == TurnLetterRestriction.ConsonantsOnly && isVowel)
            {
                feedback = LocalizationManager.Localize("Gameplay.Modifier.OnlyConsonants");
                return false;
            }
        }

        feedback = string.Empty;
        return true;
    }

    public int GetEarlyHitBonus(int turnIndex)
    {
        if (_activeModifier != MatchModifierType.EarlyHitBonus)
            return 0;

        if (turnIndex <= 1)
            return 4;

        if (turnIndex == 2)
            return 2;

        return 0;
    }

    public string GetActiveModifierLocalizedText()
    {
        switch (_activeModifier)
        {
            case MatchModifierType.TemporaryKeyLock:
                return string.Format("{0} {1}",
                    LocalizationManager.Localize("Gameplay.Modifier.TemporaryKeyLock"),
                    GetLockedLettersText());

            case MatchModifierType.EarlyHitBonus:
                return LocalizationManager.Localize("Gameplay.Modifier.EarlyHitBonus");

            case MatchModifierType.TurnLetterRestriction:
                return string.Format("{0} {1}",
                    LocalizationManager.Localize("Gameplay.Modifier.TurnRestriction"),
                    GetTurnRestrictionLocalizedText());

            default:
                return LocalizationManager.Localize("Gameplay.Modifier.None");
        }
    }

    private string GetTurnRestrictionLocalizedText()
    {
        switch (_turnRestriction)
        {
            case TurnLetterRestriction.VowelsOnly:
                return LocalizationManager.Localize("Gameplay.Modifier.OnlyVowels");

            case TurnLetterRestriction.ConsonantsOnly:
                return LocalizationManager.Localize("Gameplay.Modifier.OnlyConsonants");

            default:
                return string.Empty;
        }
    }

    private void RollLockedLetters()
    {
        _lockedLetters.Clear();

        while (_lockedLetters.Count < lockedKeyCount)
        {
            int randomIndex = UnityEngine.Random.Range(0, Alphabet.Length);
            _lockedLetters.Add(Alphabet[randomIndex]);
        }
    }

    private string GetLockedLettersText()
    {
        StringBuilder result = new StringBuilder("[");
        int index = 0;

        foreach (char lockedLetter in _lockedLetters)
        {
            result.Append(lockedLetter);
            if (index < _lockedLetters.Count - 1)
                result.Append(", ");

            index++;
        }

        result.Append("]");
        return result.ToString();
    }

    private bool IsVowel(char letter)
    {
        return Vowels.Contains(letter.ToString());
    }
}
