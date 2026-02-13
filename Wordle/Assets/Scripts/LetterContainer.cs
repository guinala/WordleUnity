using System.Collections;
using TMPro;
using UnityEngine;

public class LetterContainer : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private TextMeshPro letter;
    [SerializeField] private SpriteRenderer letterContainer;

    [Header("Settings")]
    [SerializeField] private bool animatedColorChange = true;

    private ThemeConfig _theme;

    private void Start()
    {
        ThemeManager.OnThemeChanged += ApplyTheme;
        ApplyTheme(ThemeManager.Instance != null ? ThemeManager.Instance.CurrentTheme : null);
    }

    private void OnDestroy()
    {
        ThemeManager.OnThemeChanged -= ApplyTheme;
    }

    public void Initialize()
    {
        EnsureTheme();
        if (_theme == null)
            return;

        letter.text = "";
        letterContainer.color = _theme.CellStyle.backgroundDefault;
        letter.color = _theme.CellStyle.textDefault;
    }

    public void SetLetter(char newLetter, bool isHint = false)
    {
        EnsureTheme();
        if (_theme == null)
            return;

        letter.color = isHint ? _theme.CellStyle.textHint : _theme.CellStyle.textDefault;
        letter.text = newLetter.ToString();
    }

    public char GetLetter()
    {
        return string.IsNullOrEmpty(letter.text) ? ' ' : letter.text[0];
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(letter.text);
    }

    public void SetValid()
    {
        EnsureTheme();
        if (_theme == null)
            return;

        ApplyStateColor(_theme.CellStyle.backgroundValid);
    }

    public void SetPotential()
    {
        EnsureTheme();
        if (_theme == null)
            return;

        ApplyStateColor(_theme.CellStyle.backgroundPotential);
    }

    public void SetInvalid()
    {
        EnsureTheme();
        if (_theme == null)
            return;

        ApplyStateColor(_theme.CellStyle.backgroundInvalid);
    }


    public void Preview(ThemeConfig theme, char previewLetter = 'A')
    {
        if (theme == null)
            return;

        _theme = theme;
        Initialize();
        SetLetter(previewLetter);
        SetPotential();
    }
    private void ApplyStateColor(Color targetColor)
    {
        if (!animatedColorChange || !isActiveAndEnabled)
        {
            letterContainer.color = targetColor;
            return;
        }

        StartCoroutine(ChangeColorRoutine(targetColor, 0.5f));
    }

    private void ApplyTheme(ThemeConfig theme)
    {
        _theme = theme;
        if (_theme == null)
            return;

        if (string.IsNullOrEmpty(letter.text))
            letter.color = _theme.CellStyle.textDefault;

        if (letterContainer.color.a <= 0f || letterContainer.color == default)
            letterContainer.color = _theme.CellStyle.backgroundDefault;
    }

    private void EnsureTheme()
    {
        if (_theme == null)
            _theme = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentTheme : null;
    }

    private IEnumerator ChangeColorRoutine(Color desiredColor, float duration)
    {
        Color colorInicial = letterContainer.color;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duration)
        {
            tiempoTranscurrido += Time.deltaTime;
            float porcentaje = tiempoTranscurrido / duration;
            float curva = Mathf.SmoothStep(0f, 1f, porcentaje);
            letterContainer.color = Color.Lerp(colorInicial, desiredColor, curva);
            yield return null;
        }

        letterContainer.color = desiredColor;
    }
}
