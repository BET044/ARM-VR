using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class VisualizadorTareas : MonoBehaviour
{
    [Header("Referencias a los paneles de texto")]
    public TMP_Text[] tituloTextos;
    public TMP_Text[] descripcionTextos;
    public TMP_Text[] estadoTextos;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co";
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    void Start()
    {
        StartCoroutine(CargarTareas());
    }

    IEnumerator CargarTareas()
    {
        string urlTareas = $"{supabaseUrl}/rest/v1/tareas?select=id,nombre,descripcion,dificultad&limit=3&order=id.asc";
        UnityWebRequest request = UnityWebRequest.Get(urlTareas);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar tareas: " + request.error);
            yield break;
        }

        string json = "{\"tareas\":" + request.downloadHandler.text + "}";
        TareaLista lista = JsonUtility.FromJson<TareaLista>(json);

        // Obtener tareas completadas
        string sesionId = PlayerPrefs.GetString("sesion_id");
        string urlCompletadas = $"{supabaseUrl}/rest/v1/sesion_tareas?select=tarea_id,completada&sesion_id=eq.{sesionId}";

        UnityWebRequest requestCompletadas = UnityWebRequest.Get(urlCompletadas);
        requestCompletadas.SetRequestHeader("apikey", apiKey);
        requestCompletadas.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));

        yield return requestCompletadas.SendWebRequest();

        HashSet<int> tareasCompletadas = new HashSet<int>();
        if (requestCompletadas.result == UnityWebRequest.Result.Success)
        {
            string jsonArray = requestCompletadas.downloadHandler.text;
            string wrapped = "{\"items\":" + jsonArray + "}";
            SesionTareaLista completadas = JsonUtility.FromJson<SesionTareaLista>(wrapped);

            foreach (var item in completadas.items)
            {
                if (item.completada)
                    tareasCompletadas.Add(item.tarea_id);
            }
        }

        for (int i = 0; i < lista.tareas.Length && i < 3; i++)
        {
            tituloTextos[i].text = lista.tareas[i].nombre;
            descripcionTextos[i].text = lista.tareas[i].descripcion;
            bool completada = tareasCompletadas.Contains(lista.tareas[i].id);
            estadoTextos[i].text = completada ? "Completada" : "No completada";
        }
    }

    [System.Serializable]
    public class Tarea
    {
        public int id;
        public string nombre;
        public string descripcion;
        public string dificultad;
    }

    [System.Serializable]
    public class TareaLista
    {
        public Tarea[] tareas;
    }

    [System.Serializable]
    public class SesionTarea
    {
        public int tarea_id;
        public bool completada;
    }

    [System.Serializable]
    public class SesionTareaLista
    {
        public SesionTarea[] items;
    }
}