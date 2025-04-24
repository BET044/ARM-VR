using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Security.Cryptography;

public class MenuPrincipal : MonoBehaviour
{
    public string escenaNiveles = "Niveles";
    public string escenaOpciones = "Opciones";
    public string escenaPerfil = "Perfil";
    public TextMeshProUGUI mensaje;
    public TextMeshProUGUI mensaje2;
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    private string userId;
    private string accessToken;
    private float tiempoInicioSesion;


        void Start()
    {
        if (PlayerPrefs.HasKey("session_data"))
        {
            string sessionData = PlayerPrefs.GetString("session_data");
            userId = ExtraerValor(sessionData, "\"id\":\"");
            accessToken = ExtraerValor(sessionData, "\"access_token\":\"");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(accessToken))
            {
                StartCoroutine(ObtenerNombreUsuario(userId, accessToken));

                // Verifica si ya existe un sesion_id
                string sesionId = PlayerPrefs.GetString("sesion_id");
                if (string.IsNullOrEmpty(sesionId))
                {
                    // Si no hay un sesion_id, entonces crea una nueva sesión
                    StartCoroutine(RegistrarSesion(userId, accessToken));
                }
                else
                {
                    // Si ya hay un sesion_id, muestra un mensaje de que la sesión ya está activa
                    Debug.Log("Sesión activa encontrada: " + sesionId);
                    mensaje2.text = "Sesión activa: " + sesionId;
                }
            }
            else
            {
                mensaje.text = "¡Bienvenido, Usuario!";
            }
        }
        else
        {
            mensaje.text = "¡Bienvenido, Invitado!";
        }
    }

    IEnumerator ObtenerNombreUsuario(string userId, string token)
    {
        string url = $"{supabaseUrl}/rest/v1/usuarios?select=nombre&id=eq.{userId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            string nombre = ExtraerValor(json, "\"nombre\":\"");
            mensaje.text = "¡Bienvenido, " + (string.IsNullOrEmpty(nombre) ? "Usuario" : nombre) + "!";
        }
        else
        {
            Debug.LogError("Error al obtener el nombre: " + request.error);
            mensaje.text = "¡Bienvenido, Usuario!";
        }
    }
    IEnumerator RegistrarSesion(string userId, string token)
    {
        string url = $"{supabaseUrl}/rest/v1/sesiones";
        string jsonData = $"{{\"usuario_id\":\"{userId}\",\"estado\":\"en progreso\",\"duracion\":0}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Prefer", "return=representation");  // <- esta línea es clave

        yield return request.SendWebRequest();

        string response = request.downloadHandler.text;
        Debug.Log("Respuesta completa del servidor: " + response);

        string cleanedResponse = response.Trim('[', ']');
        string sesionId = ExtraerValor(cleanedResponse, "\"id\":");

        if (!string.IsNullOrEmpty(sesionId))
        {
            PlayerPrefs.SetString("sesion_id", sesionId);
            tiempoInicioSesion = Time.realtimeSinceStartup;
            Debug.Log("Sesión iniciada: " + sesionId);
            mensaje2.text = "Sesión iniciada: " + sesionId;
        }
        else
        {
            Debug.LogError("No se encontró el ID de la sesión en la respuesta.");
        }
    }

    IEnumerator RegistrarFinSesion()
    {
        string sesionId = PlayerPrefs.GetString("sesion_id");
        if (string.IsNullOrEmpty(sesionId)) yield break;

        int duracion = Mathf.RoundToInt(Time.realtimeSinceStartup - tiempoInicioSesion);
        string url = $"{supabaseUrl}/rest/v1/sesiones?id=eq.{sesionId}";
        string jsonData = $"{{\"estado\":\"completada\",\"fecha\":\"now()\",\"duracion\":{duracion}}}";

        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Sesión finalizada correctamente");
        }
        else
        {
            Debug.LogError("Error al finalizar sesión: " + request.error);
        }
    }


    string ExtraerValor(string texto, string clave)
    {
        int inicio = texto.IndexOf(clave);
        if (inicio == -1) return null;

        inicio += clave.Length;
        int fin = texto.IndexOfAny(new char[] { ',', '\"', '}' }, inicio);
        return texto.Substring(inicio, fin - inicio);
    }

    public void BotonNiveles()
    {
        SceneManager.LoadScene(escenaNiveles);
    }

    public void BotonOpciones()
    {
        SceneManager.LoadScene(escenaOpciones);
    }

    public void BotonPerfil()
    {
        SceneManager.LoadScene(escenaPerfil);
    }

    public void BotonSalir()
    {
        StartCoroutine(RegistrarFinSesion());
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("session_data");
        PlayerPrefs.DeleteKey("sesion_id");

        Debug.Log("Sesión cerrada.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnApplicationQuit()
    {
        if (!string.IsNullOrEmpty(userId) && PlayerPrefs.HasKey("sesion_id"))
        {
            StartCoroutine(RegistrarFinSesion());
        }
    }
}
