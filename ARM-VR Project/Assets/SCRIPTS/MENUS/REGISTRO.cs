using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;
using System;

public class REGISTRO : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;
    public TMP_Dropdown roleDropdown;
    public TextMeshProUGUI statusText;

    // Credenciales     
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    [System.Serializable]
    private class SignUpRequest
    {
        public string email;
        public string password;
    }

    [System.Serializable]
    private class UserData
    {
        public string id;
        public string nombre;
        public string correo;
        public int rol_id;
    }

    [System.Serializable]
    private class SignUpResponse
    {
        public string access_token;
        public string refresh_token;
        public User user;
    }

    [System.Serializable]
    private class User
    {
        public string id;
        public string email;
    }

    public void Register()
    {
        if (!ValidateInputs())
            return;

        int selectedRole = roleDropdown.value + 1; // Asume que los roles empiezan en 1
        StartCoroutine(RegisterCoroutine(emailInput.text, passwordInput.text, nameInput.text, selectedRole));
    }

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(emailInput.text))
        {
            statusText.text = "Por favor ingresa tu email";
            return false;
        }

        if (!emailInput.text.Contains("@"))
        {
            statusText.text = "Email no válido";
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

        if (string.IsNullOrWhiteSpace(nameInput.text))
        {
            statusText.text = "Por favor ingresa tu nombre";
            return false;
        }

        return true;
    }

    IEnumerator RegisterCoroutine(string email, string password, string nombre, int rol_id)
    {
        statusText.text = "Registrando usuario...";

        // Paso 1: Registro en Auth de Supabase
        var signUpData = new SignUpRequest
        {
            email = email,
            password = password
        };

        string jsonData = JsonUtility.ToJson(signUpData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest authRequest = new UnityWebRequest(supabaseUrl + "/auth/v1/signup", "POST"))
        {
            authRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            authRequest.downloadHandler = new DownloadHandlerBuffer();
            authRequest.SetRequestHeader("Content-Type", "application/json");
            authRequest.SetRequestHeader("apikey", apiKey);

            yield return authRequest.SendWebRequest();

            if (authRequest.result != UnityWebRequest.Result.Success)
            {
                ProcessAuthError(authRequest);
                yield break;
            }

            // Procesar respuesta exitosa
            SignUpResponse response = JsonUtility.FromJson<SignUpResponse>(authRequest.downloadHandler.text);
            
            // Guardar datos de sesión
            PlayerPrefs.SetString("session_data", authRequest.downloadHandler.text);
            PlayerPrefs.SetString("access_token", response.access_token);
            PlayerPrefs.SetString("user_id", response.user.id);
            PlayerPrefs.SetString("user_email", response.user.email);
            PlayerPrefs.Save();

            // Paso 2: Guardar datos adicionales en la tabla de usuarios
            yield return StartCoroutine(SaveUserData(response.user.id, nombre, email, rol_id, response.access_token));

            SceneManager.LoadScene("MENU MAIN");
        }
    }

    IEnumerator SaveUserData(string uid, string nombre, string email, int rol_id, string token)
    {
        statusText.text = "Guardando datos del usuario...";

        var userData = new UserData
        {
            id = uid,
            nombre = nombre,
            correo = email,
            rol_id = rol_id
        };

        string jsonData = JsonUtility.ToJson(userData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest dbRequest = new UnityWebRequest(supabaseUrl + "/rest/v1/usuarios", "POST"))
        {
            dbRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            dbRequest.downloadHandler = new DownloadHandlerBuffer();
            dbRequest.SetRequestHeader("Content-Type", "application/json");
            dbRequest.SetRequestHeader("apikey", apiKey);
            dbRequest.SetRequestHeader("Authorization", "Bearer " + token);
            dbRequest.SetRequestHeader("Prefer", "return=minimal"); // Para evitar respuesta innecesaria

            yield return dbRequest.SendWebRequest();

            if (dbRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al guardar en BD: " + dbRequest.error);
                Debug.LogError("Respuesta: " + dbRequest.downloadHandler.text);
                // Considera eliminar el usuario de auth si falla el guardado en BD
            }
        }
    }

    private void ProcessAuthError(UnityWebRequest request)
    {
        Debug.LogError($"Error: {request.error}");
        Debug.LogError($"Response: {request.downloadHandler.text}");

        if (request.responseCode == 400)
        {
            // Analizar el mensaje de error para dar feedback más específico
            if (request.downloadHandler.text.Contains("already registered"))
            {
                statusText.text = "Este email ya está registrado";
            }
            else
            {
                statusText.text = "Datos de registro no válidos";
            }
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
}