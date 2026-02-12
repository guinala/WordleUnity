using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum Validity { None, Valid, Potential, Invalid }

public class Key : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Image renderer;
    [SerializeField] private TextMeshProUGUI letterText;

    [Header("Settings")]
    private Validity _validity;
    private ThemeConfig _theme;

    [Header("Events")]
    public static Action<char> OnKeyPressed;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SendKeyPressedEvent);
        ThemeManager.OnThemeChanged += ApplyTheme;
        ApplyTheme(ThemeManager.Instance != null ? ThemeManager.Instance.CurrentTheme : null);
        Initialize();
    }

    private void OnDestroy()
    {
        ThemeManager.OnThemeChanged -= ApplyTheme;
    }

    private void SendKeyPressedEvent()
    {
        OnKeyPressed?.Invoke(letterText.text[0]);
    }

    public void Initialize()
    {
        EnsureTheme();
        renderer.color = _theme.KeyboardStyle.keyDefault;
        letterText.color = _theme.KeyboardStyle.textColor;
        _validity = Validity.None;
    }

    public char GetLetter()
    {
        return letterText.text[0];
    }

    public void SetValid()
    {
        EnsureTheme();
        renderer.color = _theme.KeyboardStyle.keyValid;
        _validity = Validity.Valid;
    }

    public void SetPotential()
    {
        EnsureTheme();
        if (_validity == Validity.Valid)
            return;
        renderer.color = _theme.KeyboardStyle.keyPotential;
        _validity = Validity.Potential;
    }

    public void SetInvalid()
    {
        EnsureTheme();
        if (_validity == Validity.Valid || _validity == Validity.Potential)
            return;
        renderer.color = _theme.KeyboardStyle.keyInvalid;
        _validity = Validity.Invalid;
    }


    public void Preview(ThemeConfig theme)
    {
        if (theme == null)
            return;

        _theme = theme;
        Initialize();
        SetPotential();
    }

    public bool IsUntouched()
    {
        return _validity == Validity.None;
    }

    private void ApplyTheme(ThemeConfig theme)
    {
        _theme = theme;
        if (_theme == null)
            return;

        letterText.color = _theme.KeyboardStyle.textColor;
        if (_validity == Validity.None)
            renderer.color = _theme.KeyboardStyle.keyDefault;
    }

    private void EnsureTheme()
    {
        if (_theme == null)
            _theme = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentTheme : null;
    }
}
