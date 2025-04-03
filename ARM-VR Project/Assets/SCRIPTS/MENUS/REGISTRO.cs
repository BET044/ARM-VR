using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class REGISRO : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField nameInput;
    public TMP_Dropdown roleDropdown;  
    public TextMeshProUGUI statusText;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";  // Reemplaza con tu URL
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw"; // Reemplaza con tu API Key

    public void Register()
    {
        // Obtener el rol seleccionado desde el dropdown
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
                statusText.text = "✅ Usuario creado. Registrando en la BD...";
                Debug.Log("Usuario registrado: " + request.downloadHandler.text);

                // Extraer UID del usuario
                string uid = ExtraerUID(request.downloadHandler.text);

                // Guardar en la tabla "usuarios"
                StartCoroutine(GuardarUsuarioBD(uid, nombre, email, rol_id));
            }
            else
            {
                statusText.text = "❌ Error: " + request.error;
                Debug.LogError("Error: " + request.error);
            }
        }
    }

    IEnumerator GuardarUsuarioBD(string uid, string nombre, string email, int rol_id)
    {
        string jsonData = "{\"id\":\"" + uid + "\", \"nombre\":\"" + nombre + "\", \"correo\":\"" + email + "\", \"rol_id\":" + rol_id + "}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        using (UnityWebRequest request = new UnityWebRequest(supabaseUrl + "/rest/v1/usuarios", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("apikey", apiKey);
            request.SetRequestHeader("Authorization", "Bearer " + apiKey); // Necesario para escribir en la BD

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                statusText.text = "✅ Usuario registrado correctamente en la BD";
            }
            else
            {
                statusText.text = "❌ Error al guardar en BD: " + request.error;
                Debug.LogError("Error en BD: " + request.error);
            }
        }
    }

    string ExtraerUID(string jsonResponse)
    {
        // Buscar y extraer el "id" de la respuesta JSON (puede usarse una librería JSON como Newtonsoft)
        int index = jsonResponse.IndexOf("\"id\":\"") + 6;
        if (index < 6) return "";
        int endIndex = jsonResponse.IndexOf("\"", index);
        return jsonResponse.Substring(index, endIndex - index);
    }
}
