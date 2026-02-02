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
    private string promptWord;
    private string promptHint;

    private string word;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<string> SetAIWord()
    {
        string language = PlayerPrefs.GetString("Language", "Spanish");
        
        if(language == "Spanish")
            language = "español";
        else if(language == "English")
            language = "inglés";
        
        promptWord = "Genera otra palabra de 5 letras en " + language + ". No puede ser una palabra compuesta. No repitas palabras. Si la palabra que escoges tiene acentos, elimina esos acentos. Escribe solamente la palabra y nada más.";
        var task = new TaskCompletionSource<string>();
        StartCoroutine(SendData(task, promptWord));

        try
        {
            string response = await task.Task;
            word = response;
            return response;
        }
        
        catch (Exception e)
        {
            return "Error";
        }
    }
    
    public async Task<string> SetAIHint()
    {
        string language = PlayerPrefs.GetString("Language", "Spanish");
        
        if(language == "Spanish")
            language = "español";
        else if(language == "English")
            language = "inglés";
        
        promptHint = "Genera una pista escrita en " + language + " para ayudar a adivinar la palabra " + word + ", la pista debe ser corta pero relevante y no debe contener la palabra misma. Escribe solamente la pista y nada más.";
        var task = new TaskCompletionSource<string>();
        StartCoroutine(SendData(task, promptHint));

        try
        {
            string response = await task.Task;
            return response;
        }
        
        catch (Exception e)
        {
            return "Error";
        }
    }

    // private IEnumerator SendData(string response)
    // {
    //     WWWForm form = new WWWForm();
    //     form.AddField("parameter", prompt);
    //     UnityWebRequest request = UnityWebRequest.Post(url, form);
    //     yield return request.SendWebRequest();
    //
    //     if (request.result == UnityWebRequest.Result.Success)
    //     {
    //         response = request.downloadHandler.text;
    //     }
    //     else
    //     {
    //         response = "Error";
    //     }
    //     UIManager.Instance.HideLoading();
    // }
    private IEnumerator SendData(TaskCompletionSource<string> tcs, string prompt)
    {
        WWWForm form = new WWWForm();
        Debug.Log(prompt);
        form.AddField("parameter", prompt);
        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            string response;
            if (request.result == UnityWebRequest.Result.Success)
            {
                response = request.downloadHandler.text;
                tcs.SetResult(response);
            }
            else
            {
                response = "Error";
                tcs.SetResult(response);
            }

            UIManager.Instance.HideLoading();
        }
    }
}
