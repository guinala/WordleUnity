using UnityEngine;

[CreateAssetMenu(fileName = "ThemeConfig", menuName = "Wordle/Theme Config")]
public class ThemeConfig : ScriptableObject
{
    [System.Serializable]
    public class CellTheme
    {
        public Color backgroundDefault = Color.white;
        public Color backgroundValid = new Color(0.41f, 0.71f, 0.31f);
        public Color backgroundPotential = new Color(0.79f, 0.67f, 0.20f);
        public Color backgroundInvalid = Color.gray;
        public Color textDefault = Color.black;
        public Color textHint = Color.gray;
    }

    [System.Serializable]
    public class KeyboardTheme
    {
        public Color keyDefault = Color.white;
        public Color keyValid = new Color(0.41f, 0.71f, 0.31f);
        public Color keyPotential = new Color(0.79f, 0.67f, 0.20f);
        public Color keyInvalid = Color.gray;
        public Color textColor = Color.black;
    }

    [Header("Theme")]
    [SerializeField] private string themeId = "classic";
    [SerializeField] private string displayName = "Classic";
    [SerializeField] private CellTheme cellTheme = new CellTheme();
    [SerializeField] private KeyboardTheme keyboardTheme = new KeyboardTheme();

    public string ThemeId => themeId;
    public string DisplayName => displayName;
    public CellTheme CellStyle => cellTheme;
    public KeyboardTheme KeyboardStyle => keyboardTheme;
}
