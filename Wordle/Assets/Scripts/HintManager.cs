using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class HintManager : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private GameObject keyboard;
    private Key[] keys;

    [Header("Text Elements")]
    [SerializeField] private TextMeshProUGUI keyboardPriceText;
    [SerializeField] private TextMeshProUGUI letterPriceText;
    [SerializeField] private TextMeshProUGUI textHintPriceText;
    [SerializeField] private TextMeshProUGUI hintText;

    [Header("Settings")]
    [SerializeField] private int keyboardHintPrice;
    [SerializeField] private int letterHintPrice;
    [SerializeField] private int textHintPrice;
    [SerializeField] private bool textHintGiven;

    private readonly List<int> letterHintGivenIndices = new List<int>();
    private bool shouldReset;

    private void Awake()
    {
        keys = keyboard.GetComponentsInChildren<Key>();
    }

    private void Start()
    {
        UpdatePricesUI();
        GameManager.OnGameStateChanged += GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback += Clear;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameStateChangedCallback;
        GameManager.OnGameBackButtonCallback -= Clear;
    }

    private void GameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Game:
                if (shouldReset)
                {
                    textHintGiven = false;
                    hintText.text = "";
                    letterHintGivenIndices.Clear();
                    shouldReset = false;
                }
                UpdatePricesUI();
                break;

            case GameState.LevelComplete:
            case GameState.GameOver:
                shouldReset = true;
                break;
        }
    }

    private void Clear()
    {
        textHintGiven = false;
        hintText.text = "";
    }

    public void KeyboardHint()
    {
        int finalPrice = GetFinalPrice(keyboardHintPrice);
        if (DataManager.instance.GetCoins() < finalPrice)
            return;

        string secretWord = WordManager.instance.GetSecretWord();

        List<Key> untouchedKeys = new List<Key>();

        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].IsUntouched())
                untouchedKeys.Add(keys[i]);
        }

        List<Key> t_untouchedKeys = new List<Key>(untouchedKeys);

        for (int i = 0; i < untouchedKeys.Count; i++)
        {
            if (secretWord.Contains(untouchedKeys[i].GetLetter()))
                t_untouchedKeys.Remove(untouchedKeys[i]);
        }

        if (t_untouchedKeys.Count <= 0)
            return;

        int randomIndex = Random.Range(0, t_untouchedKeys.Count);
        t_untouchedKeys[randomIndex].SetInvalid();

        DataManager.instance.RemoveCoins(finalPrice);
        DataManager.instance.RegisterHintUsed();
        UpdatePricesUI();
    }

    public void LetterHint()
    {
        int finalPrice = GetFinalPrice(letterHintPrice);
        if (DataManager.instance.GetCoins() < finalPrice)
            return;

        if (letterHintGivenIndices.Count >= 5)
        {
            Debug.Log("All hints given");
            return;
        }

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

        DataManager.instance.RemoveCoins(finalPrice);
        DataManager.instance.RegisterHintUsed();
        UpdatePricesUI();
    }

    public void ShowHintPanel()
    {
        UIManager.Instance.ShowHintUI();
        if (!textHintGiven)
            UIManager.Instance.ShowMainHintPanel();
        else
            UIManager.Instance.ShowGivenHintPanel();
    }

    public async void TextHint()
    {
        int finalPrice = GetFinalPrice(textHintPrice);
        if (DataManager.instance.GetCoins() < finalPrice)
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
        DataManager.instance.RemoveCoins(finalPrice);
        DataManager.instance.RegisterHintUsed();
        UpdatePricesUI();
    }

    private int GetFinalPrice(int basePrice)
    {
        return PerkManager.Instance == null ? basePrice : PerkManager.Instance.GetDiscountedHintPrice(basePrice);
    }

    private void UpdatePricesUI()
    {
        keyboardPriceText.text = GetFinalPrice(keyboardHintPrice).ToString();
        letterPriceText.text = GetFinalPrice(letterHintPrice).ToString();
        textHintPriceText.text = GetFinalPrice(textHintPrice).ToString();
    }
}
