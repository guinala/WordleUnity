using System;
using UnityEngine;

public enum EnvironmentWeather
{
    Unknown,
    Clear,
    Rain,
    Clouds,
    Snow
}

public enum TimeBand
{
    Unknown,
    Morning,
    Afternoon,
    Night
}

public enum HintType
{
    Keyboard,
    Letter,
    Text
}

public class EnvironmentState : MonoBehaviour
{
    public static EnvironmentState Instance => EnsureInstance();

    public static event Action OnEnvironmentChanged;

    public EnvironmentWeather Weather { get; private set; } = EnvironmentWeather.Unknown;
    public TimeBand CurrentTimeBand { get; private set; } = TimeBand.Unknown;
    public bool HasLocation { get; private set; }
    public bool HasNetwork { get; private set; } = true;

    private static EnvironmentState instance;

    private static EnvironmentState EnsureInstance()
    {
        if (instance != null)
        {
            return instance;
        }

        instance = FindObjectOfType<EnvironmentState>();
        if (instance != null)
        {
            return instance;
        }

        GameObject go = new GameObject("EnvironmentState");
        instance = go.AddComponent<EnvironmentState>();
        return instance;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void SetState(EnvironmentWeather weather, TimeBand timeBand, bool hasLocation, bool hasNetwork)
    {
        Weather = weather;
        CurrentTimeBand = timeBand;
        HasLocation = hasLocation;
        HasNetwork = hasNetwork;
        OnEnvironmentChanged?.Invoke();
    }

    public static TimeBand GetCurrentLocalTimeBand()
    {
        int hour = DateTime.Now.Hour;

        if (hour >= 6 && hour < 12)
            return TimeBand.Morning;

        if (hour >= 12 && hour < 20)
            return TimeBand.Afternoon;

        return TimeBand.Night;
    }

    public int GetHintPrice(int basePrice, HintType hintType)
    {
        int adjustedPrice = basePrice;

        if (Weather == EnvironmentWeather.Rain && hintType != HintType.Text)
            adjustedPrice += 1;

        if (CurrentTimeBand == TimeBand.Night && hintType == HintType.Text)
            adjustedPrice = Mathf.Max(1, adjustedPrice - 1);

        return adjustedPrice;
    }

    public float GetWordCheckDelay(float baseDelay)
    {
        if (CurrentTimeBand == TimeBand.Night)
            return baseDelay + 0.2f;

        if (CurrentTimeBand == TimeBand.Morning)
            return Mathf.Max(0.2f, baseDelay - 0.1f);

        return baseDelay;
    }

    public int GetScoreBonus()
    {
        return Weather == EnvironmentWeather.Clear && CurrentTimeBand == TimeBand.Morning ? 1 : 0;
    }

    public string GetHudSummary(string language)
    {
        string weatherText = Weather switch
        {
            EnvironmentWeather.Clear => language == "Spanish" ? "Despejado" : "Clear",
            EnvironmentWeather.Rain => language == "Spanish" ? "Lluvia" : "Rain",
            EnvironmentWeather.Clouds => language == "Spanish" ? "Nublado" : "Cloudy",
            EnvironmentWeather.Snow => language == "Spanish" ? "Nieve" : "Snow",
            _ => language == "Spanish" ? "Sin datos" : "No data"
        };

        string timeBandText = CurrentTimeBand switch
        {
            TimeBand.Morning => language == "Spanish" ? "Mañana" : "Morning",
            TimeBand.Afternoon => language == "Spanish" ? "Tarde" : "Afternoon",
            TimeBand.Night => language == "Spanish" ? "Noche" : "Night",
            _ => language == "Spanish" ? "Desconocida" : "Unknown"
        };

        return language == "Spanish"
            ? $"Clima: {weatherText} | Franja: {timeBandText}"
            : $"Weather: {weatherText} | Time: {timeBandText}";
    }
}
