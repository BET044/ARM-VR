using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;
using System;

public class LOGIN : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI statusText;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    [System.Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class UserMetadata
    {
        public string email;
    }

    [System.Serializable]
    private class User
    {
        public string id;
        public string email;
        public UserMetadata user_metadata;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string access_token;
        public string refresh_token;
        public User user;
        public string expires_in;
    }

    public void Login()
    {
        if (!ValidateInputs())
            return;

        StartCoroutine(LoginCoroutine(emailInput.text, passwordInput.text));
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(emailInput.text))
        {
            statusText.text = "Por favor ingresa tu email";
            return false;
        }

        if (string.IsNullOrWhiteSpace(passwordInput.text))
        {
            statusText.text = "Por favor ingresa tu contraseña";
            return false;
        }

        if (passwordInput.text.Length < 6)
        {
            statusText.text = "La contraseña debe tener al menos 6 caracteres";
            return false;
        }

        return true;
    }

    IEnumerator LoginCoroutine(string email, string password)
    {
        statusText.text = "Iniciando sesión...";

        var loginData = new LoginRequest
        {
            email = email,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);
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
                ProcessSuccessfulLogin(request.downloadHandler.text);
            }
            else
            {
                ProcessLoginError(request);
            }
        }
    }

    private void ProcessSuccessfulLogin(string responseJson)
    {
        try
        {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(responseJson);
            
            // Guardar datos de sesión
            PlayerPrefs.SetString("session_data", responseJson);
            PlayerPrefs.SetString("access_token", response.access_token);
            PlayerPrefs.SetString("user_id", response.user?.id ?? "");
            PlayerPrefs.SetString("user_email", response.user?.email ?? response.user?.user_metadata?.email ?? "");
            
            PlayerPrefs.Save();
            
            Debug.Log($"Login exitoso. User ID: {response.user?.id}");
            SceneManager.LoadScene("MENU MAIN");
        }
        catch (Exception e)
        {
            statusText.text = "Error procesando la respuesta";
            Debug.LogError($"Error parsing login response: {e.Message}");
        }
    }

    private void ProcessLoginError(UnityWebRequest request)
    {
        Debug.LogError($"Error: {request.error}");
        Debug.LogError($"Response: {request.downloadHandler.text}");

        if (request.responseCode == 400)
        {
            statusText.text = "Email o contraseña incorrectos";
        }
        else if (request.responseCode >= 500)
        {
            statusText.text = "Error del servidor. Intenta más tarde";
        }
        else
        {
            statusText.text = "Error al conectar con el servidor";
        }
    }

    public void BotonInvitado()
    {
        // Limpiar cualquier sesión previa
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("session_data");
        
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