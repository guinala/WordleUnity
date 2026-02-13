using System;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    private static ThemeManager _instance;

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ThemeManager>();

            return _instance;
        }
    }

    [SerializeField] private ThemeConfig fallbackTheme;
    private ThemeConfig _currentTheme;

    public static Action<ThemeConfig> OnThemeChanged;

    public ThemeConfig CurrentTheme => _currentTheme != null ? _currentTheme : fallbackTheme;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
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
