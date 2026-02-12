using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class HintManager : MonoBehaviour
{
    private enum PurchaseHintType
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

    private readonly List<int> letterHintGivenIndices = new List<int>();
    private bool shouldReset;
    private PurchaseHintType pendingHintType = PurchaseHintType.None;
    private bool textHintInProgress;
    private bool waitingForRewardedAd;

    private int CurrentKeyboardHintPrice => GetEnvironmentAdjustedPrice(GetKeyboardHintPrice(), HintType.Keyboard);
    private int CurrentLetterHintPrice => GetEnvironmentAdjustedPrice(GetLetterHintPrice(), HintType.Letter);
    private int CurrentTextHintPrice => GetEnvironmentAdjustedPrice(GetTextHintPrice(), HintType.Text);

    private void Awake()
    {
        keys = keyboard.GetComponentsInChildren<Key>();
    }

    private void Start()
    {
        RefreshHintPrices();
        RefreshPurchasePreview(PurchaseHintType.None);

        GameManager.OnGameStateChanged += GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback += Clear;
        EnvironmentState.OnEnvironmentChanged += RefreshHintPrices;
        DataManager.OnHintUsageChanged += RefreshHintPrices;
        AdsController.OnRewardedAdCompleted += OnRewardedAdCompleted;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback -= Clear;
        EnvironmentState.OnEnvironmentChanged -= RefreshHintPrices;
        DataManager.OnHintUsageChanged -= RefreshHintPrices;
        AdsController.OnRewardedAdCompleted -= OnRewardedAdCompleted;
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
                RefreshHintPrices();
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
        textHintInProgress = false;
        waitingForRewardedAd = false;
        hintText.text = "";
        letterHintGivenIndices.Clear();
        pendingHintType = PurchaseHintType.None;
        RefreshPurchasePreview(PurchaseHintType.None);
        RefreshHintPrices();
    }

    private int GetKeyboardHintPrice() => keyboardHintPrice + DataManager.instance.GetKeyboardHintUses() * keyboardHintPriceStep;
    private int GetLetterHintPrice() => letterHintPrice + DataManager.instance.GetLetterHintUses() * letterHintPriceStep;
    private int GetTextHintPrice() => textHintPrice + DataManager.instance.GetTextHintUses() * textHintPriceStep;
    private int GetBetHintPrice() => betHintPrice + DataManager.instance.GetBetHintUses() * betHintPriceStep;

    private int GetEnvironmentAdjustedPrice(int basePrice, HintType hintType)
    {
        return EnvironmentState.Instance == null
            ? basePrice
            : EnvironmentState.Instance.GetHintPrice(basePrice, hintType);
    }

    private void RefreshHintPrices()
    {
        UpdatePricesUI();
        RefreshPurchasePreview(pendingHintType);
    }

    private void UpdatePricesUI()
    {
        if (keyboardPriceText != null)
            keyboardPriceText.text = CurrentKeyboardHintPrice.ToString();

        if (letterPriceText != null)
            letterPriceText.text = CurrentLetterHintPrice.ToString();

        if (textHintPriceText != null)
            textHintPriceText.text = "AD";

        if (betHintPriceText != null)
            betHintPriceText.text = GetBetHintPrice().ToString();
    }

    public void PreviewKeyboardHint() => RefreshPurchasePreview(PurchaseHintType.Keyboard);
    public void PreviewLetterHint() => RefreshPurchasePreview(PurchaseHintType.Letter);
    public void PreviewTextHint() => RefreshPurchasePreview(PurchaseHintType.Text);
    public void PreviewBetHint() => RefreshPurchasePreview(PurchaseHintType.Bet);

    private void RefreshPurchasePreview(PurchaseHintType hintType)
    {
        pendingHintType = hintType;

        if (hintPreviewTitleText == null || hintPreviewCostText == null || hintPreviewImpactText == null)
            return;

        switch (hintType)
        {
            case PurchaseHintType.Keyboard:
                hintPreviewTitleText.text = "Pista Teclado";
                hintPreviewCostText.text = $"Coste: {CurrentKeyboardHintPrice} monedas";
                hintPreviewImpactText.text = $"Impacto: -{keyboardHintScorePenalty} score";
                break;
            case PurchaseHintType.Letter:
                hintPreviewTitleText.text = "Pista Letra";
                hintPreviewCostText.text = $"Coste: {CurrentLetterHintPrice} monedas";
                hintPreviewImpactText.text = $"Impacto: -{letterHintScorePenalty} score";
                break;
            case PurchaseHintType.Text:
                hintPreviewTitleText.text = "Pista IA";
                hintPreviewCostText.text = "Coste: ver anuncio recompensado";
                hintPreviewImpactText.text = $"Impacto: -{textHintScorePenalty} score";
                break;
            case PurchaseHintType.Bet:
                hintPreviewTitleText.text = "Pista Apuesta";
                hintPreviewCostText.text = $"Coste: {GetBetHintPrice()} monedas";
                hintPreviewImpactText.text =
                    $"Impacto: {betHintBenefitChance * 100f:0}% +{betHintCoinReward} monedas / {100f - betHintBenefitChance * 100f:0}% -{betHintCoinLoss} monedas";
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
            case PurchaseHintType.Keyboard:
                ApplyKeyboardHint();
                break;
            case PurchaseHintType.Letter:
                ApplyLetterHint();
                break;
            case PurchaseHintType.Text:
                RequestRewardedTextHint();
                break;
            case PurchaseHintType.Bet:
                ApplyBetHint();
                break;
        }
    }

    // Backwards-compatible callbacks if old buttons still point to direct methods.
    public void KeyboardHint() => ApplyKeyboardHint();
    public void LetterHint() => ApplyLetterHint();

    // Explicit rewarded flow for IA hint: request ad first, reward callback grants hint without coin cost.
    public void RequestRewardedTextHint()
    {
        SyncRewardedAdState();

        if (textHintGiven || textHintInProgress || waitingForRewardedAd)
        {
            return;
        }

        if (AdsController.Instance == null)
        {
            return;
        }

        waitingForRewardedAd = AdsController.Instance.ShowAdvertisement();
    }

    private void SyncRewardedAdState()
    {
        if (!waitingForRewardedAd || AdsController.Instance == null)
        {
            return;
        }

        if (!AdsController.Instance.IsRewardedAdFlowInProgress())
        {
            waitingForRewardedAd = false;
        }
    }

    private void ApplyKeyboardHint()
    {
        int currentPrice = CurrentKeyboardHintPrice;
        if (DataManager.instance.GetCoins() < currentPrice)
        {
            return;
        }

        string secretWord = WordManager.instance.GetSecretWord();
        List<Key> invalidUntouchedKeys = new List<Key>();

        for (int i = 0; i < keys.Length; i++)
        {
            if (!keys[i].IsUntouched())
                continue;

            if (!secretWord.Contains(keys[i].GetLetter()))
                invalidUntouchedKeys.Add(keys[i]);
        }

        if (invalidUntouchedKeys.Count <= 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, invalidUntouchedKeys.Count);
        invalidUntouchedKeys[randomIndex].SetInvalid();

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterHintUsed();
        DataManager.instance.RegisterKeyboardHintUse(keyboardHintScorePenalty);
        RefreshHintPrices();
        RefreshPurchasePreview(PurchaseHintType.Keyboard);
    }

    private void ApplyLetterHint()
    {
        int currentPrice = CurrentLetterHintPrice;
        if (DataManager.instance.GetCoins() < currentPrice)
        {
            return;
        }

        if (letterHintGivenIndices.Count >= 5)
        {
            return;
        }

        List<int> letterHintNotGivenIndices = new List<int>();

        for (int i = 0; i < 5; i++)
        {
            if (!letterHintGivenIndices.Contains(i))
                letterHintNotGivenIndices.Add(i);
        }

        if (letterHintNotGivenIndices.Count <= 0)
        {
            return;
        }

        WordContainer currentWordContainer = InputManager.instance.GetCurrentWordContainer();
        string secretWord = WordManager.instance.GetSecretWord();

        int randomIndex = letterHintNotGivenIndices[Random.Range(0, letterHintNotGivenIndices.Count)];
        letterHintGivenIndices.Add(randomIndex);
        currentWordContainer.AddAsHint(randomIndex, secretWord[randomIndex]);

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterHintUsed();
        DataManager.instance.RegisterLetterHintUse(letterHintScorePenalty);
        RefreshHintPrices();
        RefreshPurchasePreview(PurchaseHintType.Letter);
    }

    public void ApplyBetHint()
    {
        int currentPrice = GetBetHintPrice();
        if (DataManager.instance.GetCoins() < currentPrice)
        {
            return;
        }

        DataManager.instance.RemoveCoins(currentPrice);
        DataManager.instance.RegisterHintUsed();
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

        RefreshHintPrices();
        RefreshPurchasePreview(PurchaseHintType.Bet);
    }

    public void ShowHintPanel()
    {
        SyncRewardedAdState();
        UIManager.Instance.ShowHintUI();
        RefreshHintPrices();

        if (!textHintGiven)
            UIManager.Instance.ShowMainHintPanel();
        else
            UIManager.Instance.ShowGivenHintPanel();
    }

    public void TextHint()
    {
        RequestRewardedTextHint();
    }

    private void OnRewardedAdCompleted()
    {
        if (!waitingForRewardedAd)
        {
            return;
        }

        waitingForRewardedAd = false;
        _ = GrantRewardedTextHintAsync();
    }

    private async System.Threading.Tasks.Task GrantRewardedTextHintAsync()
    {
        if (textHintGiven || textHintInProgress)
        {
            return;
        }

        textHintInProgress = true;
        UIManager.Instance.HideMainHintPanel();
        UIManager.Instance.ShowLoading(true);

        string hint = await APIManager.instance.SetAIHint();

        UIManager.Instance.ShowLoading(false);
        UIManager.Instance.ShowGivenHintPanel();
        hintText.text = hint.Trim();

        textHintGiven = true;
        DataManager.instance.RegisterHintUsed();
        DataManager.instance.RegisterTextHintUse(textHintScorePenalty);
        textHintInProgress = false;
        RefreshHintPrices();
        RefreshPurchasePreview(PurchaseHintType.Text);
    }
}
