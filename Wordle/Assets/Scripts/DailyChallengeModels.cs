using System;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class DailyChallengeResponse
{
    public string palabra;
    public string pista;
    public string tema;
    public string regla;
    public string idioma;
}

public class DailyChallengeData
{
    public string Word;
    public string Hint;
    public string Theme;
    public string Rule;
    public string Language;
}

public static class DailyChallengeParser
{
    public static bool TryParse(string rawJson, out DailyChallengeData data)
    {
        data = null;

        if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "Error")
            return false;

        string cleanJson = CleanJson(rawJson);

        DailyChallengeResponse response;
        try
        {
            response = JsonUtility.FromJson<DailyChallengeResponse>(cleanJson);
        }
        catch (Exception)
        {
            return false;
        }

        if (response == null)
            return false;

        data = new DailyChallengeData
        {
            Word = (response.palabra ?? string.Empty).Trim().ToUpper(),
            Hint = (response.pista ?? string.Empty).Trim(),
            Theme = (response.tema ?? string.Empty).Trim(),
            Rule = (response.regla ?? string.Empty).Trim(),
            Language = (response.idioma ?? string.Empty).Trim().ToLower()
        };

        return true;
    }

    public static string CleanJson(string rawJson)
    {
        string cleanJson = rawJson.Trim();

        if (cleanJson.StartsWith("```") && cleanJson.EndsWith("```"))
        {
            cleanJson = cleanJson.Replace("```json", string.Empty)
                                 .Replace("```", string.Empty)
                                 .Trim();
        }

        return cleanJson;
    }
}

public static class DailyChallengeValidator
{
    private const int WordLength = 5;
    private static readonly Regex AllowedCharactersRegex = new Regex("^[A-Za-zÑñ]+$", RegexOptions.Compiled);

    public static bool IsValid(DailyChallengeData challenge, string expectedLanguage)
    {
        if (challenge == null)
            return false;

        if (string.IsNullOrEmpty(challenge.Word) || challenge.Word.Length != WordLength)
            return false;

        if (!AllowedCharactersRegex.IsMatch(challenge.Word))
            return false;

        if (string.IsNullOrWhiteSpace(challenge.Hint) || string.IsNullOrWhiteSpace(challenge.Theme) || string.IsNullOrWhiteSpace(challenge.Rule))
            return false;

        string normalizedExpectedLanguage = NormalizeLanguage(expectedLanguage);
        if (string.IsNullOrEmpty(challenge.Language))
            return true;

        return challenge.Language == normalizedExpectedLanguage;
    }

    private static string NormalizeLanguage(string language)
    {
        if (language == "Spanish")
            return "español";

        if (language == "English")
            return "inglés";

        return language.ToLower();
    }
}
