//private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";     private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

public class LOGIN : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    [System.Serializable]
    public class UserMetadata
    {
        public string email;
    }

    [System.Serializable]
    public class User
    {
        public string id;
        public UserMetadata user_metadata;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string access_token;
        public User user;
    }

    public void Login()
    {
        StartCoroutine(LoginCoroutine(emailInput.text, passwordInput.text));
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        statusText.text = "Iniciando sesión...";
        string jsonData = "{\"email\":\"" + email + "\", \"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(supabaseUrl + "/auth/v1/token?grant_type=password", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success || request.responseCode == 200)
            {
                Debug.Log("Login exitoso: " + request.downloadHandler.text);
                PlayerPrefs.SetString("session_data", request.downloadHandler.text);
                // Cargar la siguiente escena
                SceneManager.LoadScene("MENU MAIN");
            }
            else
            {
                Debug.LogError("Error: " + request.error);
                Debug.LogError("Respuesta: " + request.downloadHandler.text);
                statusText.text = "Error al iniciar sesión";
            }
        }
    }
    
        public void BotonInvitado()
    {
        SceneManager.LoadScene("MENU MAIN");
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("session_data");
        statusText.text = "Sesión cerrada";
        Debug.Log("Sesión cerrada.");
    }
}
