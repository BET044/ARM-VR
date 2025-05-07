using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Globalization;

public class EvaluadorTarea : MonoBehaviour
{
    private float tiempoInicio;
    private GameObject objeto;
    private Transform objetivoFinal;
    private bool tareaActiva = false;
    private float tiempoTotal;

    [Header("Datos de Supabase")]
    [SerializeField] private int tareaId;
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";
    private string accessToken;
    private string userId;

    [Header("UI TMP")]
    [SerializeField] private TextMeshProUGUI tiempoTMP;
    [SerializeField] private TextMeshProUGUI resultadoTMP;
    [SerializeField] private TextMeshProUGUI TESTDEBUG;
    [SerializeField] private GameObject menuscore;
    [SerializeField] private GameObject botonrojo;
    [SerializeField] private GameObject botonver;

    [Header("Límite de tiempo opcional")]
    [SerializeField] private float tiempoLimite = 10f;
    [SerializeField] private float precisionMinima = 10f;

    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAlerta = Color.red;

    void Start()
    {
        if (PlayerPrefs.HasKey("session_data"))
        {
            string sessionData = PlayerPrefs.GetString("session_data");
            accessToken = ExtraerValor(sessionData, "\"access_token\":\"");
            userId = ExtraerValor(sessionData, "\"id\":\"");
        }
    }

    void Update()
    {
        if (tareaActiva && tiempoTMP != null)
        {
            float tiempoActual = Time.time - tiempoInicio;
            tiempoTMP.text = $"{tiempoActual:F2}s";

            tiempoTMP.color = tiempoActual > tiempoLimite ? colorAlerta : colorNormal;
        }
    }

    public void IniciarTarea(GameObject objetoAgarrado, Transform objetivo)
    {
        objeto = objetoAgarrado;
        objetivoFinal = objetivo;
        tiempoInicio = Time.time;
        tareaActiva = true;

        if (tiempoTMP != null)
        {
            tiempoTMP.text = "0.00s";
            tiempoTMP.color = colorNormal;
        }

        Debug.Log("Tarea iniciada.");
    }

    public void FinalizarTarea()
    {
        if (!tareaActiva || objeto == null || objetivoFinal == null) return;

        tiempoTotal = Time.time - tiempoInicio;
        float precision = Vector3.Distance(objeto.transform.position, objetivoFinal.position);

        bool pasoTiempo = tiempoTotal <= tiempoLimite;
        bool pasoPrecision = precision <= precisionMinima;
        bool tareaCompletada = pasoTiempo && pasoPrecision;

        if (resultadoTMP != null)
        {
            menuscore.SetActive(true);
            botonrojo.SetActive(false);
            botonver.SetActive(true);
            tiempoTMP.text = "";

            string resultadoTexto = tareaCompletada ? "¡Tarea completada exitosamente!" : "No se cumplió el objetivo.\n";
            resultadoTMP.text = $"Tiempo: {tiempoTotal:F2}s\nPrecisión: {precision:F3}u\n\n{resultadoTexto}";
        }

        Debug.Log($"Tarea finalizada.\nTiempo: {tiempoTotal:F2}s\nPrecisión: {precision:F3} unidades\nCompletada: {tareaCompletada}");

        tareaActiva = false;

        StartCoroutine(RegistrarTareaEnDB(tiempoTotal, tareaCompletada));
        StartCoroutine(RegistrarResultadoEnDB(tiempoTotal, precision));

        objeto = null;
        objetivoFinal = null;
    }

    IEnumerator RegistrarTareaEnDB(float tiempo, bool completada)
    {
        string sesionId = PlayerPrefs.GetString("sesion_id");
        if (string.IsNullOrEmpty(sesionId))
        {
            Debug.LogError("No hay sesion_id disponible.");
            yield break;
        }

        string url = $"{supabaseUrl}/rest/v1/sesion_tareas";
        string jsonData = $"{{" +
            $"\"sesion_id\":\"{sesionId}\"," +
            $"\"tarea_id\":{tareaId}," +
            $"\"completada\":{completada.ToString().ToLower()}," +
            $"\"tiempo_empleado\":{Mathf.RoundToInt(tiempo)}" +
            $"}}";

        Debug.Log("JSON enviado a sesion_tareas: " + jsonData);

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
            Debug.Log("Tarea registrada correctamente en sesion_tareas.");
        }
        else
        {
            Debug.LogError("Error al registrar tarea: " + request.error);
            Debug.LogError("Respuesta servidor: " + request.downloadHandler.text);
        }
    }

    IEnumerator RegistrarResultadoEnDB(float tiempo, float precision)
    {
        string sesionId = PlayerPrefs.GetString("sesion_id");
        if (string.IsNullOrEmpty(sesionId) || string.IsNullOrEmpty(userId))
        {
            Debug.LogError("Faltan sesion_id o userId para registrar resultado.");
            yield break;
        }

        string url = $"{supabaseUrl}/rest/v1/resultados_tareas";
        string jsonData = $"{{" +
            $"\"sesion_id\":\"{sesionId}\"," +
            $"\"tarea_id\":{tareaId}," +
            $"\"usuario_id\":\"{userId}\"," +
            $"\"tiempo\":{tiempo.ToString(CultureInfo.InvariantCulture)}," +
            $"\"precision\":{precision.ToString(CultureInfo.InvariantCulture)}" +
            $"}}";

        Debug.Log("JSON enviado a resultados_tareas: " + jsonData);

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
            Debug.Log("Resultado registrado correctamente en resultados_tareas.");
            TESTDEBUG.text = "Resultado registrado correctamente.";
        }
        else
        {
            Debug.LogError("Error al registrar resultado: " + request.error);
            Debug.LogError("Respuesta servidor: " + request.downloadHandler.text);
            TESTDEBUG.text = "Error: " + request.downloadHandler.text;
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
}
