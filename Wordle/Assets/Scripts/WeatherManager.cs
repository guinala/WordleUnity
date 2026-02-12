using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Android;

[System.Serializable]
public class WeatherMain {
    public float temp;
}

[System.Serializable]
public class WeatherResponse {
    public WeatherMain main;
    public Weather[] weather;
}

[System.Serializable]
public class Weather {
    public string main;
    public string description;
}

public class WeatherManager : MonoBehaviour
{
    private string apiKey = "a22080def78dafb5cbe88dd9290f85a8";
    private float latitude;
    private float longitude;

    [Header("Elements")] 
    [SerializeField] private GameObject rainMode;

    void Start()
    {
        StartCoroutine(AskForPermissions());
    }
    
    IEnumerator AskForPermissions()
    {
        #if UNITY_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
                {
                    Permission.RequestUserPermission(Permission.FineLocation);
                    yield return new WaitForSeconds(0.2f); 
                }
        #endif
        
        StartCoroutine(GetLocation());
    }
    
    IEnumerator GetLocation()
    {
        if (!Input.location.isEnabledByUser)
        {
            Debug.Log("El usuario no tiene activada la ubicación");
            ApplyFallback(hasLocation: false, hasNetwork: Application.internetReachability != NetworkReachability.NotReachable);
            yield break;
        }

        Input.location.Start();
        
        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.Log("Timeout al iniciar GPS");
            ApplyFallback(hasLocation: false, hasNetwork: Application.internetReachability != NetworkReachability.NotReachable);
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("No se pudo obtener la ubicación");
            ApplyFallback(hasLocation: false, hasNetwork: Application.internetReachability != NetworkReachability.NotReachable);
            yield break;
        }
        
        latitude = Input.location.lastData.latitude;
        longitude = Input.location.lastData.longitude;

        Debug.Log($"Lat: {latitude} Lon: {longitude}");
        StartCoroutine(GetWeather());
    }


    IEnumerator GetWeather()
    {
        bool hasNetwork = Application.internetReachability != NetworkReachability.NotReachable;
        if (!hasNetwork)
        {
            Debug.Log("Sin red disponible para consultar clima");
            ApplyFallback(hasLocation: true, hasNetwork: false);
            yield break;
        }

        string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apiKey}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al obtener clima: " + request.error);
                ApplyFallback(hasLocation: true, hasNetwork: false);
            }
            else
            {
                WeatherResponse weatherData = JsonUtility.FromJson<WeatherResponse>(request.downloadHandler.text);
                Debug.Log($"Temperatura: {weatherData.main.temp}°C  Condición: {weatherData.weather[0].main}");

                EnvironmentWeather weather = ParseWeather(weatherData.weather[0].main);
                TimeBand timeBand = EnvironmentState.GetCurrentLocalTimeBand();
                EnvironmentState.Instance.SetState(weather, timeBand, hasLocation: true, hasNetwork: true);

                if(weather == EnvironmentWeather.Rain)
                    rainMode.SetActive(true);
                else
                    rainMode.SetActive(false);
            }
        }
    }

    private EnvironmentWeather ParseWeather(string weatherMain)
    {
        return weatherMain switch
        {
            "Clear" => EnvironmentWeather.Clear,
            "Rain" => EnvironmentWeather.Rain,
            "Clouds" => EnvironmentWeather.Clouds,
            "Snow" => EnvironmentWeather.Snow,
            _ => EnvironmentWeather.Unknown
        };
    }

    private void ApplyFallback(bool hasLocation, bool hasNetwork)
    {
        rainMode.SetActive(false);
        EnvironmentState.Instance.SetState(EnvironmentWeather.Unknown, EnvironmentState.GetCurrentLocalTimeBand(), hasLocation, hasNetwork);
    }
}
