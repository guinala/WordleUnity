using System;
using Assets.SimpleLocalization.Scripts;
using UnityEngine;

public class MultiLanguage : MonoBehaviour
{
    public static MultiLanguage Instance;
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LocalizationManager.Read();

        switch (Application.systemLanguage)
        {
            case SystemLanguage.English:
                LocalizationManager.Language = "English";
                break;
            
            case SystemLanguage.Spanish:
                LocalizationManager.Language = "Spanish";
                break;
        }
    }

    public void Language(string language)
    {
        LocalizationManager.Language = language;
    }
}
