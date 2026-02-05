using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

enum Validity {None, Valid, Potential, Invalid}
public class Key : MonoBehaviour
{
    [Header("Elements")] 
    [SerializeField] private Image renderer;
    [SerializeField] private TextMeshProUGUI letterText;

    [Header("Settings")] 
    private Validity _validity;
    
    [Header("Events")]
    public static Action<char> OnKeyPressed;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(SendKeyPressedEvent);
        Initialize();
    }

    private void SendKeyPressedEvent()
    {
        OnKeyPressed?.Invoke(letterText.text[0]);
    }

    public void Initialize()
    {
        renderer.color = Color.white;
        _validity = Validity.None;
    }
    
    public char GetLetter()
    {
        return letterText.text[0];
    }
    
    public void SetValid()
    {
        renderer.color = Color.yellowGreen;
        _validity = Validity.Valid;
    }
    
    public void SetPotential()
    {
        if (_validity == Validity.Valid)
            return;
        renderer.color = Color.yellowNice;
        _validity = Validity.Potential;
    }
    
    public void SetInvalid()
    {
        if(_validity == Validity.Valid || _validity == Validity.Potential)
            return;
        renderer.color = Color.gray;
        _validity = Validity.Invalid;
    }

    public bool IsUntouched()
    {
        return _validity == Validity.None;
    }
}
