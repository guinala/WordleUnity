using System;
using System.Collections.Generic;
using UnityEngine;

public class WordContainer : MonoBehaviour
{
    [Header("Elements")] 
    private LetterContainer[] _letterContainers;

    [Header("Settings")] private int currentLetterIndex;

    private void Awake()
    {
        _letterContainers = GetComponentsInChildren<LetterContainer>();
        //Initialize();
    }

    public void Initialize()
    {
        currentLetterIndex = 0;
        
        for (int i = 0; i < _letterContainers.Length; i++)
        {
            _letterContainers[i].Initialize();
        }
    }

    public void Add(char letter)
    {
        _letterContainers[currentLetterIndex].SetLetter(letter);
        currentLetterIndex++;
    }

    public string GetWord()
    {
        string word = "";
        for (int i = 0; i < _letterContainers.Length; i++)
            word += _letterContainers[i].GetLetter().ToString();

        return word;
    }

    public void AddAsHint(int letterIndex, char letter)
    {
        _letterContainers[letterIndex].SetLetter(letter, true);
    }

    public bool RemoveLetter()
    {
        if (currentLetterIndex <= 0)
        {
            return false;
        }

        currentLetterIndex--;
        _letterContainers[currentLetterIndex].Initialize();
        return true;
    }

    public bool IsComplete()
    {
        return currentLetterIndex >= 5;
    }

    public void Colorize(string secreWord)
    {
        List<char> chars = new List<char>(secreWord.ToCharArray());

        for (int i = 0; i < _letterContainers.Length; i++)
        {
            char letterToCheck = _letterContainers[i].GetLetter();

            if (letterToCheck == secreWord[i]) //Valid case
            {
                _letterContainers[i].SetValid();
                chars.Remove(letterToCheck);
            }
            else if (chars.Contains(letterToCheck)) //Potential case
            {
                _letterContainers[i].SetPotential();
                chars.Remove(letterToCheck);
            }
            else //Failed case
            {
                _letterContainers[i].SetInvalid();
            }
        }
    }
}
