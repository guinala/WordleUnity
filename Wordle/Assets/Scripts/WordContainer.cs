using System.Collections.Generic;
using UnityEngine;

public class WordContainer : MonoBehaviour
{
    [Header("Elements")]
    private LetterContainer[] _letterContainers;

    [Header("Settings")]
    private bool[] _lockedIndices;

    private void Awake()
    {
        _letterContainers = GetComponentsInChildren<LetterContainer>();
        _lockedIndices = new bool[_letterContainers.Length];
    }

    public void Initialize()
    {
        for (int i = 0; i < _letterContainers.Length; i++)
        {
            _lockedIndices[i] = false;
            _letterContainers[i].Initialize();
        }
    }

    public void Add(char letter)
    {
        int targetIndex = GetNextWritableIndex(0);
        if (targetIndex < 0)
            return;

        _letterContainers[targetIndex].SetLetter(letter);
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
        if (letterIndex < 0 || letterIndex >= _letterContainers.Length)
            return;

        _lockedIndices[letterIndex] = true;
        _letterContainers[letterIndex].SetLetter(letter, true);
    }

    public bool RemoveLetter()
    {
        int targetIndex = GetPreviousWritableFilledIndex(_letterContainers.Length - 1);
        if (targetIndex < 0)
            return false;

        _letterContainers[targetIndex].Initialize();
        return true;
    }

    public bool IsComplete()
    {
        for (int i = 0; i < _letterContainers.Length; i++)
        {
            if (_letterContainers[i].IsEmpty())
                return false;
        }

        return true;
    }

    public void Colorize(string secreWord)
    {
        List<char> chars = new List<char>(secreWord.ToCharArray());

        for (int i = 0; i < _letterContainers.Length; i++)
        {
            char letterToCheck = _letterContainers[i].GetLetter();

            if (letterToCheck == secreWord[i])
            {
                _letterContainers[i].SetValid();
                chars.Remove(letterToCheck);
            }
            else if (chars.Contains(letterToCheck))
            {
                _letterContainers[i].SetPotential();
                chars.Remove(letterToCheck);
            }
            else
            {
                _letterContainers[i].SetInvalid();
            }
        }
    }

    private int GetNextWritableIndex(int start)
    {
        for (int i = start; i < _letterContainers.Length; i++)
        {
            if (_lockedIndices[i])
                continue;

            if (_letterContainers[i].IsEmpty())
                return i;
        }

        return -1;
    }

    private int GetPreviousWritableFilledIndex(int start)
    {
        for (int i = start; i >= 0; i--)
        {
            if (_lockedIndices[i])
                continue;

            if (!_letterContainers[i].IsEmpty())
                return i;
        }

        return -1;
    }
}
