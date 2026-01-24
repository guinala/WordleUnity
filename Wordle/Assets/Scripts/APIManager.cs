using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager instance;
    
    [SerializeField] private string url;
    [SerializeField] private string prompt;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public string SetAIWord()
    {
        string response = "";
        StartCoroutine(SendData(response));
        return response;
    }

    private IEnumerator SendData(string response)
    {
        WWWForm form = new WWWForm();
        form.AddField("parameter", prompt);
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            response = request.downloadHandler.text;
        }
        else
        {
            response = "Error";
        }
        
        Debug.Log(response);
    }
}
