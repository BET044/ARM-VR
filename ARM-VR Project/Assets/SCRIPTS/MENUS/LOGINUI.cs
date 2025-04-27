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
                string response = request.downloadHandler.text;
                Debug.Log("Login exitoso: " + response);
                
                // Guardar toda la sesión
                PlayerPrefs.SetString("session_data", response);
                
                // Extraer y guardar específicamente el user_id
                string userId = ExtraerValor(response, "\"id\":\"");
                if (!string.IsNullOrEmpty(userId))
                {
                    PlayerPrefs.SetString("user_id", userId);
                }
                
                // Extraer y guardar el access token
                string accessToken = ExtraerValor(response, "\"access_token\":\"");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    PlayerPrefs.SetString("access_token", accessToken);
                }
                
                PlayerPrefs.Save();
                Debug.Log($"User ID: {PlayerPrefs.GetString("user_id")}");
                Debug.Log($"Access Token: {PlayerPrefs.GetString("access_token")}");
                Debug.Log($"Session Data: {PlayerPrefs.GetString("session_data")}");
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

    // Método para extraer valores del JSON
    string ExtraerValor(string texto, string clave)
    {
        int inicio = texto.IndexOf(clave);
        if (inicio == -1) return null;

        inicio += clave.Length;
        int fin = texto.IndexOfAny(new char[] { ',', '\"', '}' }, inicio);
        return texto.Substring(inicio, fin - inicio);
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
