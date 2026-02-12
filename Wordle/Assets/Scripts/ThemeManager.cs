using System;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance;

    [SerializeField] private ThemeConfig fallbackTheme;
    private ThemeConfig _currentTheme;

    public static Action<ThemeConfig> OnThemeChanged;

    public ThemeConfig CurrentTheme => _currentTheme != null ? _currentTheme : fallbackTheme;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _currentTheme = fallbackTheme;
    }

    public void ApplyTheme(ThemeConfig theme)
    {
        if (theme == null)
            return;

        _currentTheme = theme;
        OnThemeChanged?.Invoke(_currentTheme);
    }
}
