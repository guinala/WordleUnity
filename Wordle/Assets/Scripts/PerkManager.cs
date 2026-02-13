using UnityEngine;
using System.Collections;

public class PerkManager : MonoBehaviour
{
    public static PerkManager Instance;

    [Header("Settings")]
    [SerializeField, Range(0f, 0.8f)] private float hintDiscountPercent = 0.2f;
    [SerializeField] private int minimumExactLettersForExtraAttempt = 2;

    private bool revealLetterApplied;
    private bool extraAttemptConsumed;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(WaitForManagersAndSubscribe());
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }

    private IEnumerator WaitForManagersAndSubscribe()
    {
        while (DataManager.instance == null || GameManager.Instance == null)
            yield return null;

        GameManager.OnGameStateChanged += OnGameStateChanged;

        if (GameManager.Instance.IsGameState())
            OnGameStateChanged(GameState.Game);
    }

    public int GetDiscountedHintPrice(int basePrice)
    {
        if (DataManager.instance == null || !DataManager.instance.IsPerkEquipped(PerkType.HintDiscount))
            return basePrice;

        int discountedPrice = Mathf.RoundToInt(basePrice * (1f - hintDiscountPercent));
        return Mathf.Max(1, discountedPrice);
    }

    public bool CanGrantConditionalExtraAttempt(int exactLettersCount)
    {
        if (DataManager.instance == null)
            return false;

        if (!DataManager.instance.IsPerkEquipped(PerkType.ConditionalExtraAttempt) || extraAttemptConsumed)
            return false;

        if (exactLettersCount < minimumExactLettersForExtraAttempt)
            return false;

        extraAttemptConsumed = true;
        return true;
    }

    private void OnGameStateChanged(GameState gameState)
    {
        if (gameState != GameState.Game)
            return;

        revealLetterApplied = false;
        extraAttemptConsumed = false;

        ApplyRoundStartPerks();
    }

    private void ApplyRoundStartPerks()
    {
        if (revealLetterApplied)
            return;

        if (DataManager.instance == null)
            return;

        if (!DataManager.instance.IsPerkEquipped(PerkType.RevealedLetter))
            return;

        if (InputManager.instance == null)
            return;

        InputManager.instance.RevealPerkLetter();
        revealLetterApplied = true;
    }
}
