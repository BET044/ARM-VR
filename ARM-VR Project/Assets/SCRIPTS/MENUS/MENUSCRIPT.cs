using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class MenuPrincipal : MonoBehaviour
{
   
    public string escenaNiveles = "Niveles";
    public string escenaOpciones = "Opciones";
    public string escenaPerfil = "Perfil";
    public TextMeshProUGUI mensaje;

    // Supabase
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    void Start()
    {
        if (PlayerPrefs.HasKey("session_data"))
        {
            string sessionData = PlayerPrefs.GetString("session_data");
            string userId = ExtraerValor(sessionData, "\"id\":\"");
            string accessToken = ExtraerValor(sessionData, "\"access_token\":\"");

            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(accessToken))
            {
                StartCoroutine(ObtenerNombreUsuario(userId, accessToken));
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

    string ExtraerValor(string texto, string clave)
    {
        int inicio = texto.IndexOf(clave);
        if (inicio == -1) return null;

        inicio += clave.Length;
        int fin = texto.IndexOf("\"", inicio);
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
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("session_data");

        Debug.Log("Sesión cerrada.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}