using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class APIManager : MonoBehaviour
{
    public static APIManager instance;

    [SerializeField] private string url;
    private string promptWord;
    private string promptHint;
    private string promptDailyChallenge;

    private string word;
    private string dailyHint;

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

        if (language == "Spanish")
            language = "español";
        else if (language == "English")
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

        catch (Exception)
        {
            return "Error";
        }
    }

    public async Task<string> SetDailyChallenge()
    {
        string language = PlayerPrefs.GetString("Language", "Spanish");

        if (language == "Spanish")
            language = "español";
        else if (language == "English")
            language = "inglés";

        promptDailyChallenge = "Genera un desafío diario para Wordle en " + language + ". " +
                               "Responde SOLO un JSON válido sin markdown ni texto extra con el formato exacto: " +
                               "{\"palabra\":\"XXXXX\",\"pista\":\"...\",\"tema\":\"...\",\"regla\":\"...\",\"idioma\":\"" + language + "\"}. " +
                               "La palabra debe tener 5 letras y solo caracteres alfabéticos.";

        var task = new TaskCompletionSource<string>();
        StartCoroutine(SendData(task, promptDailyChallenge));

        try
        {
            return await task.Task;
        }
        catch (Exception)
        {
            return "Error";
        }
    }

    public async Task<string> SetAIHint()
    {
        if (!string.IsNullOrEmpty(dailyHint))
            return dailyHint;

        string language = PlayerPrefs.GetString("Language", "Spanish");

        if (language == "Spanish")
            language = "español";
        else if (language == "English")
            language = "inglés";

        promptHint = "Genera una pista escrita en " + language + " para ayudar a adivinar la palabra " + word + ", la pista debe ser corta pero relevante y no debe contener la palabra misma. Escribe solamente la pista y nada más.";
        var task = new TaskCompletionSource<string>();
        StartCoroutine(SendData(task, promptHint));

        try
        {
            return await task.Task;
        }

        catch (Exception)
        {
            return "Error";
        }
    }

    public void SetCurrentWordAndHint(string currentWord, string currentHint)
    {
        word = currentWord;
        dailyHint = currentHint;
    }

    public void ClearDailyHint()
    {
        dailyHint = string.Empty;
    }

    private IEnumerator SendData(TaskCompletionSource<string> tcs, string prompt)
    {
        WWWForm form = new WWWForm();
        Debug.Log(prompt);
        form.AddField("parameter", prompt);
        using (UnityWebRequest request = UnityWebRequest.Post(url, form))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                tcs.SetResult(request.downloadHandler.text);
            }
            else
            {
                tcs.SetResult("Error");
            }

            UIManager.Instance.HideLoading();
        }
    }
}
