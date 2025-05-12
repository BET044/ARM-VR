using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class PerfilUsuario : MonoBehaviour
{
    [Header("Supabase")]
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    [Header("UI Usuario")]
    public TMP_Text nombreUsuario;
    public TMP_Text correoUsuario;
    public TMP_Text fechaRegistro;

    [Header("UI Historial")]
    public GameObject historialItemPrefab;
    public Transform historialContent;

    void Start()
    {
        StartCoroutine(CargarInformacionUsuario());
        StartCoroutine(CargarHistorialIntentos());
    }

    IEnumerator CargarInformacionUsuario()
    {
        string usuarioId = PlayerPrefs.GetString("user_id");
        string url = $"{supabaseUrl}/rest/v1/usuarios?id=eq.{usuarioId}&select=nombre,correo,fecha_registro";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text.Trim('[', ']'); // solo 1 usuario esperado
            Usuario usuario = JsonUtility.FromJson<Usuario>(json);
            nombreUsuario.text = usuario.nombre;
            correoUsuario.text = usuario.correo;
            fechaRegistro.text = "Desde: " + usuario.fecha_registro.Split('T')[0];
        }
        else
        {
            Debug.LogError("Error al cargar usuario: " + request.error);
        }
    }

    IEnumerator CargarHistorialIntentos()
    {
        string usuarioId = PlayerPrefs.GetString("user_id");
        string url = $"{supabaseUrl}/rest/v1/resultados_tareas?usuario_id=eq.{usuarioId}&select=fecha,tiempo,precision,tarea_id,tareas(nombre)&order=fecha.desc";

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = "{\"items\":" + request.downloadHandler.text + "}";
            HistorialLista lista = JsonUtility.FromJson<HistorialLista>(json);

            foreach (var intento in lista.items)
            {
                GameObject item = Instantiate(historialItemPrefab, historialContent);
                TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();

                textos[0].text = intento.tareas.nombre;
                textos[1].text = intento.fecha.Split('T')[0];
                textos[2].text = $"{intento.tiempo:F2}s";
                textos[3].text = $"{intento.precision:F2}%";
            }
        }
        else
        {
            Debug.LogError("Error al cargar historial: " + request.error);
        }
    }

    [System.Serializable]
    public class Usuario
    {
        public string nombre;
        public string correo;
        public string fecha_registro;
    }

    [System.Serializable]
    public class HistorialItem
    {
        public string fecha;
        public double tiempo;
        public double precision;
        public int tarea_id;
        public TareaInfo tareas;
    }

    [System.Serializable]
    public class TareaInfo
    {
        public string nombre;
    }

    [System.Serializable]
    public class HistorialLista
    {
        public HistorialItem[] items;
    }
}
