using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Text;

public class REGISRO : MonoBehaviour
{
    [System.Serializable]
    public class SignUpResponse
    {
        public string access_token;
        public User user;
    }

    [System.Serializable]
    public class User
    {
        public string id;
    }

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;
    public TMP_Dropdown roleDropdown;
    public TextMeshProUGUI statusText;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    public void Register()
    {
        int selectedRole = roleDropdown.value + 1;
        StartCoroutine(RegisterCoroutine(emailInput.text, passwordInput.text, nameInput.text, selectedRole));
    }

    IEnumerator RegisterCoroutine(string email, string password, string nombre, int rol_id)
    {
        statusText.text = "Registrando usuario...";
        string jsonData = "{\"email\":\"" + email + "\", \"password\":\"" + password + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(supabaseUrl + "/auth/v1/signup", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Usuario registrado: " + request.downloadHandler.text);

                SignUpResponse response = JsonUtility.FromJson<SignUpResponse>(request.downloadHandler.text);
                string uid = response.user.id;
                string accessToken = response.access_token;

                statusText.text = "Usuario creado. Guardando en la base de datos...";
                StartCoroutine(GuardarUsuarioBD(uid, nombre, email, rol_id, accessToken));
                
                statusText.text = "Login exitoso";
                Debug.Log("Respuesta: " + request.downloadHandler.text);
                PlayerPrefs.SetString("session_data", request.downloadHandler.text);

                SceneManager.LoadScene("MENU MAIN");
            }
            else
            {
                statusText.text = "Error al crear usuario: " + request.error;
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    IEnumerator GuardarUsuarioBD(string uid, string nombre, string email, int rol_id, string token)
    {
        string jsonData = "{\"id\":\"" + uid + "\", \"nombre\":\"" + nombre + "\", \"correo\":\"" + email + "\", \"rol_id\":" + rol_id + "}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        Debug.Log("Enviando a BD: " + jsonData);

        using (UnityWebRequest request = new UnityWebRequest(supabaseUrl + "/rest/v1/usuarios", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                statusText.text = "Usuario registrado correctamente en la base de datos.";
                Debug.Log("Registro en BD exitoso.");
            }
            else
            {
                statusText.text = "Error al guardar en BD: " + request.error;
                Debug.LogError("Error en BD: " + request.error);
                Debug.LogError("Respuesta: " + request.downloadHandler.text);
            }
        }
    }
}
