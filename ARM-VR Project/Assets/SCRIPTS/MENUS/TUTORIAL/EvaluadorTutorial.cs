using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class EvaluadorTutorial : MonoBehaviour
{
    [SerializeField] private int tareaId = 999; // ID fijo del tutorial
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";
    private string accessToken;
    private string userId;

    private void Start()
    {
        if (PlayerPrefs.HasKey("session_data"))
        {
            string sessionData = PlayerPrefs.GetString("session_data");
            accessToken = ExtraerValor(sessionData, "\"access_token\":\"");
            userId = ExtraerValor(sessionData, "\"id\":\"");
        }
    }

    public void RegistrarTutorial(TextMeshProUGUI mensajeTMP, TextMeshProUGUI debugTMP)
    {
        StartCoroutine(EnviarRegistro(mensajeTMP, debugTMP));
    }

    IEnumerator EnviarRegistro(TextMeshProUGUI mensajeTMP, TextMeshProUGUI debugTMP)
    {
        string sesionId = PlayerPrefs.GetString("sesion_id");
        if (string.IsNullOrEmpty(sesionId) || string.IsNullOrEmpty(userId))
        {
            debugTMP.text = "Faltan datos de sesión o usuario.";
            yield break;
        }

        string url = $"{supabaseUrl}/rest/v1/resultados_tareas";
        string jsonData = $"{{" +
            $"\"sesion_id\":{sesionId}," +
            $"\"tarea_id\":{tareaId}," +
            $"\"usuario_id\":\"{userId}\"," +
            $"\"tiempo\":1," +
            $"\"precision\":1" +
            $"}}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);
        request.SetRequestHeader("Prefer", "return=representation");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //mensajeTMP.text += "\nRegistro en resultados_tareas exitoso.";

            // Registro adicional en sesion_tareas
            string urlSesionTarea = $"{supabaseUrl}/rest/v1/sesion_tareas";
            string jsonSesionTarea = $"{{" +
                $"\"sesion_id\":{sesionId}," +
                $"\"tarea_id\":{tareaId}," +
                $"\"completada\":true," +
                $"\"tiempo_empleado\":1" +
                $"}}";

            UnityWebRequest reqSesionTarea = new UnityWebRequest(urlSesionTarea, "POST");
            byte[] bodySesionTarea = System.Text.Encoding.UTF8.GetBytes(jsonSesionTarea);
            reqSesionTarea.uploadHandler = new UploadHandlerRaw(bodySesionTarea);
            reqSesionTarea.downloadHandler = new DownloadHandlerBuffer();
            reqSesionTarea.SetRequestHeader("Content-Type", "application/json");
            reqSesionTarea.SetRequestHeader("apikey", apiKey);
            reqSesionTarea.SetRequestHeader("Authorization", "Bearer " + accessToken);
            reqSesionTarea.SetRequestHeader("Prefer", "return=representation");

            yield return reqSesionTarea.SendWebRequest();

            if (reqSesionTarea.result == UnityWebRequest.Result.Success)
            {
                //mensajeTMP.text += "\nRegistro en sesion_tareas exitoso.";
            }
            else
            {
                debugTMP.text += $"\nError en sesion_tareas: {reqSesionTarea.downloadHandler.text}";
            }
        }
        else
        {
            debugTMP.text = "❌ Error DB: " + request.downloadHandler.text;
        }
    }

    private string ExtraerValor(string texto, string clave)
    {
        int inicio = texto.IndexOf(clave);
        if (inicio == -1) return null;
        inicio += clave.Length;
        int fin = texto.IndexOfAny(new char[] { ',', '\"', '}' }, inicio);
        return texto.Substring(inicio, fin - inicio);
    }
}
