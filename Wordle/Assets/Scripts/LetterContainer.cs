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

    public void Initialize()
    {
        letter.text = "";
        letterContainer.color = Color.white;
    }

    public void SetLetter(char letter, bool isHint = false)
    {
        if (isHint)
            this.letter.color = Color.gray;
        else
        {
            this.letter.color = Color.black;
        }
        
        this.letter.text = letter.ToString();
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
        if(!animatedColorChange) letterContainer.color = Color.yellowGreen;
        else
        {
            StartCoroutine(ChangeColorRoutine(Color.yellowGreen, 0.5f));
        }
    }

    public void SetPotential()
    {
        if(!animatedColorChange) letterContainer.color = Color.yellowNice;
        else
        {
            StartCoroutine(ChangeColorRoutine(Color.yellowNice, 0.5f));
        }
    }
    
    public void SetInvalid()
    {
        if(!animatedColorChange) letterContainer.color = Color.gray;
        else
        {
            StartCoroutine(ChangeColorRoutine(Color.gray, 0.5f));
        }
    }
    
    private IEnumerator ChangeColorRoutine(Color desiredColor, float duration)
    {
        Color colorInicial = letterContainer.color;
        float tiempoTranscurrido = 0f;
    
        while (tiempoTranscurrido < duration)
        {
            tiempoTranscurrido += Time.deltaTime;
            float porcentaje = tiempoTranscurrido / duration;
        
            // Aplica una curva suave (ease in-out)
            float curva = Mathf.SmoothStep(0f, 1f, porcentaje);
        
            letterContainer.color = Color.Lerp(colorInicial, desiredColor, curva);
        
            yield return null;
        }
    
        letterContainer.color = desiredColor;
    }
}
