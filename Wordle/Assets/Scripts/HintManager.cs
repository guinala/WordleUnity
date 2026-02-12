using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class HintManager : MonoBehaviour
{
    private enum HintType
    {
        None,
        Keyboard,
        Letter,
        Text,
        Bet,
    }

    [Header("Elements")]
    [SerializeField] private GameObject keyboard;
    private Key[] keys;

    [Header("Price Text Elements")]
    [SerializeField] private TextMeshProUGUI keyboardPriceText;
    [SerializeField] private TextMeshProUGUI letterPriceText;
    [SerializeField] private TextMeshProUGUI textHintPriceText;
    [SerializeField] private TextMeshProUGUI betHintPriceText;

    [Header("Hint Text Elements")]
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Purchase Preview Elements")]
    [SerializeField] private TextMeshProUGUI hintPreviewTitleText;
    [SerializeField] private TextMeshProUGUI hintPreviewCostText;
    [SerializeField] private TextMeshProUGUI hintPreviewImpactText;

    [Header("Base Prices")]
    [SerializeField] private int keyboardHintPrice = 10;
    [SerializeField] private int letterHintPrice = 15;
    [SerializeField] private int textHintPrice = 25;
    [SerializeField] private int betHintPrice = 20;

    [Header("Dynamic Price Increment Per Use")]
    [SerializeField] private int keyboardHintPriceStep = 4;
    [SerializeField] private int letterHintPriceStep = 5;
    [SerializeField] private int textHintPriceStep = 8;
    [SerializeField] private int betHintPriceStep = 6;

    [Header("Score Penalties")]
    [SerializeField] private int keyboardHintScorePenalty = 1;
    [SerializeField] private int letterHintScorePenalty = 2;
    [SerializeField] private int textHintScorePenalty = 2;
    [SerializeField] private int betHintScorePenalty = 1;

    [Header("Bet Hint Settings")]
    [Range(0f, 1f)] [SerializeField] private float betHintBenefitChance = 0.5f;
    [SerializeField] private int betHintCoinReward = 20;
    [SerializeField] private int betHintCoinLoss = 20;
    [SerializeField] private int betHintPenaltyReductionOnWin = 1;

    [Header("State")]
    [SerializeField] private bool textHintGiven;

    private bool shouldReset;
    private HintType pendingHintType = HintType.None;
    private readonly List<int> letterHintGivenIndices = new List<int>();

    private void Awake()
    {
        keys = keyboard.GetComponentsInChildren<Key>();
    }

    private void Start()
    {
        RefreshHintPrices();
        RefreshPurchasePreview(HintType.None);
        GameManager.OnGameStateChanged += GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback += Clear;
        DataManager.OnHintUsageChanged += RefreshHintPrices;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback -= Clear;
        DataManager.OnHintUsageChanged -= RefreshHintPrices;
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Game:
                if (shouldReset)
                {
                    ResetLocalHintState();
                    shouldReset = false;
                }
                break;

            case GameState.LevelComplete:
            case GameState.GameOver:
                shouldReset = true;
                break;
        }
    }

    private void Clear()
    {
        ResetLocalHintState();
    }

    private void ResetLocalHintState()
    {
        textHintGiven = false;
        hintText.text = "";
        letterHintGivenIndices.Clear();
        pendingHintType = HintType.None;
        RefreshPurchasePreview(HintType.None);
    }

    private int GetKeyboardHintPrice() => keyboardHintPrice + DataManager.instance.GetKeyboardHintUses() * keyboardHintPriceStep;
    private int GetLetterHintPrice() => letterHintPrice + DataManager.instance.GetLetterHintUses() * letterHintPriceStep;
    private int GetTextHintPrice() => textHintPrice + DataManager.instance.GetTextHintUses() * textHintPriceStep;
    private int GetBetHintPrice() => betHintPrice + DataManager.instance.GetBetHintUses() * betHintPriceStep;

    private void RefreshHintPrices()
    {
        if (keyboardPriceText != null)
            keyboardPriceText.text = GetKeyboardHintPrice().ToString();

        if (letterPriceText != null)
            letterPriceText.text = GetLetterHintPrice().ToString();

        if (textHintPriceText != null)
            textHintPriceText.text = GetTextHintPrice().ToString();

        if (betHintPriceText != null)
            betHintPriceText.text = GetBetHintPrice().ToString();
    }

    public void PreviewKeyboardHint() => RefreshPurchasePreview(HintType.Keyboard);
    public void PreviewLetterHint() => RefreshPurchasePreview(HintType.Letter);
    public void PreviewTextHint() => RefreshPurchasePreview(HintType.Text);
    public void PreviewBetHint() => RefreshPurchasePreview(HintType.Bet);

    private void RefreshPurchasePreview(HintType hintType)
    {
        pendingHintType = hintType;

        if (hintPreviewTitleText == null || hintPreviewCostText == null || hintPreviewImpactText == null)
            return;

        switch (hintType)
        {
            case HintType.Keyboard:
                hintPreviewTitleText.text = "Pista Teclado";
                hintPreviewCostText.text = $"Coste: {GetKeyboardHintPrice()} monedas";
                hintPreviewImpactText.text = $"Impacto: -{keyboardHintScorePenalty} score";
                break;
            case HintType.Letter:
                hintPreviewTitleText.text = "Pista Letra";
                hintPreviewCostText.text = $"Coste: {GetLetterHintPrice()} monedas";
                hintPreviewImpactText.text = $"Impacto: -{letterHintScorePenalty} score";
                break;
            case HintType.Text:
                hintPreviewTitleText.text = "Pista IA";
                hintPreviewCostText.text = $"Coste: {GetTextHintPrice()} monedas";
                hintPreviewImpactText.text = $"Impacto: -{textHintScorePenalty} score";
                break;
            case HintType.Bet:
                hintPreviewTitleText.text = "Pista Apuesta";
                hintPreviewCostText.text = $"Coste: {GetBetHintPrice()} monedas";
                hintPreviewImpactText.text = $"Impacto: {betHintBenefitChance * 100f:0}% +{betHintCoinReward} monedas / {100f - betHintBenefitChance * 100f:0}% -{betHintCoinLoss} monedas";
                break;
            default:
                hintPreviewTitleText.text = "Selecciona una pista";
                hintPreviewCostText.text = "Coste: --";
                hintPreviewImpactText.text = "Impacto: --";
                break;
        }
    }

    public void ConfirmHintPurchase()
    {
        switch (pendingHintType)
        {
            case HintType.Keyboard:
                ApplyKeyboardHint();
                break;
            case HintType.Letter:
                ApplyLetterHint();
                break;
            case HintType.Text:
                TextHint();
                break;
            case HintType.Bet:
                ApplyBetHint();
                break;
        }
    }

    // Backwards-compatible callbacks if old buttons still point to direct methods.
    public void KeyboardHint() => ApplyKeyboardHint();
    public void LetterHint() => ApplyLetterHint();

    private void ApplyKeyboardHint()
    {
        int currentPrice = GetKeyboardHintPrice();
        if (DataManager.instance.GetCoins() < currentPrice)
            return;

        string secretWord = WordManager.instance.GetSecretWord();

        List<Key> untouchedKeys = new List<Key>();

        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].IsUntouched())
                untouchedKeys.Add(keys[i]);
        }

        List<Key> invalidUntouchedKeys = new List<Key>(untouchedKeys);

        for (int i = 0; i < untouchedKeys.Count; i++)
        {
            if (secretWord.Contains(untouchedKeys[i].GetLetter()))
                invalidUntouchedKeys.Remove(untouchedKeys[i]);
        }

        if (invalidUntouchedKeys.Count <= 0)
            return;

        int randomIndex = Random.Range(0, invalidUntouchedKeys.Count);
        invalidUntouchedKeys[randomIndex].SetInvalid();

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterKeyboardHintUse(keyboardHintScorePenalty);
        RefreshPurchasePreview(HintType.Keyboard);
    }

    private void ApplyLetterHint()
    {
        int currentPrice = GetLetterHintPrice();
        if (DataManager.instance.GetCoins() < currentPrice)
            return;

        if (letterHintGivenIndices.Count >= 5)
            return;

        List<int> letterHintNotGivenIndices = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            if (!letterHintGivenIndices.Contains(i))
                letterHintNotGivenIndices.Add(i);
        }

        WordContainer currentWordContainer = InputManager.instance.GetCurrentWordContainer();
        string secretWord = WordManager.instance.GetSecretWord();

        int randomIndex = letterHintNotGivenIndices[Random.Range(0, letterHintNotGivenIndices.Count)];
        letterHintGivenIndices.Add(randomIndex);

        currentWordContainer.AddAsHint(randomIndex, secretWord[randomIndex]);

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterLetterHintUse(letterHintScorePenalty);
        RefreshPurchasePreview(HintType.Letter);
    }

    public void ApplyBetHint()
    {
        int currentPrice = GetBetHintPrice();
        if (DataManager.instance.GetCoins() < currentPrice)
            return;

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterBetHintUse(betHintScorePenalty);

        bool winsBet = Random.value <= betHintBenefitChance;
        if (winsBet)
        {
            DataManager.instance.AddCoins(betHintCoinReward);
            DataManager.instance.ReduceMatchHintPenalty(betHintPenaltyReductionOnWin);
        }
        else
        {
            DataManager.instance.RemoveCoins(betHintCoinLoss);
        }

        RefreshPurchasePreview(HintType.Bet);
    }

    public void ShowHintPanel()
    {
        UIManager.Instance.ShowHintUI();
        RefreshHintPrices();

        if (!textHintGiven)
        {
            UIManager.Instance.ShowMainHintPanel();
        }
        else
        {
            UIManager.Instance.ShowGivenHintPanel();
        }
    }

    public async void TextHint()
    {
        int currentPrice = GetTextHintPrice();
        if (DataManager.instance.GetCoins() < currentPrice)
            return;

        if (textHintGiven)
            return;

        UIManager.Instance.HideMainHintPanel();
        UIManager.Instance.ShowLoading(true);
        string hint = await APIManager.instance.SetAIHint();
        hint = hint.Trim();

        UIManager.Instance.ShowGivenHintPanel();
        hintText.text = hint;

        textHintGiven = true;
        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterTextHintUse(textHintScorePenalty);
        RefreshPurchasePreview(HintType.Text);
    }
}
