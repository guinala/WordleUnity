using System.Collections;
using TMPro;
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
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.Log("No se pudo obtener la ubicación");
            yield break;
        }
        
        float lat = Input.location.lastData.latitude;
        float lon = Input.location.lastData.longitude;

        Debug.Log($"Lat: {lat} Lon: {lon}");
        StartCoroutine(GetWeather());
    }


    IEnumerator GetWeather()
    {
        string url = $"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&units=metric&appid={apiKey}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al obtener clima: " + request.error);
            }
            else
            {
                WeatherResponse weatherData = JsonUtility.FromJson<WeatherResponse>(request.downloadHandler.text);
                Debug.Log($"Temperatura: {weatherData.main.temp}°C  Condición: {weatherData.weather[0].main}");
                if(weatherData.weather[0].main == "Rain")
                    rainMode.SetActive(true);
            }
        }
    }
}
