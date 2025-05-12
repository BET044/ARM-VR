using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class TareasYLeaderboard : MonoBehaviour
{
    [Header("Supabase")]
    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    [Header("UI Tareas")]
    public TMP_Text tituloTarea;
    public TMP_Text descripcionTarea;
    public Button botonAnterior;
    public Button botonSiguiente;

    [Header("UI Leaderboard")]
    public Transform leaderboardContent;
    public GameObject leaderboardItemPrefab;

    private Tarea[] tareas;
    private int indiceActual = 0;

    void Start()
    {
        StartCoroutine(CargarTareas());
        botonAnterior.onClick.AddListener(() => CambiarTarea(-1));
        botonSiguiente.onClick.AddListener(() => CambiarTarea(1));
    }

    void CambiarTarea(int cambio)
    {
        if (tareas == null || tareas.Length == 0) return;

        indiceActual = Mathf.Clamp(indiceActual + cambio, 0, tareas.Length - 1);
        MostrarTareaActual();
        StartCoroutine(CargarLeaderboard(tareas[indiceActual].id));
    }

    void MostrarTareaActual()
    {
        if (tareas == null || tareas.Length == 0) return;

        Tarea actual = tareas[indiceActual];
        tituloTarea.text = actual.nombre;

        if (actual.id == 1)
            descripcionTarea.text = "Completado";
        else if (actual.id == 2)
            descripcionTarea.text = "Menor distancia de error";
        else if (actual.id == 3)
            descripcionTarea.text = "Mayor cantidad de cajas movidas";
        else
            descripcionTarea.text = actual.descripcion;
    }

    IEnumerator CargarTareas()
    {
        string url = $"{supabaseUrl}/rest/v1/tareas?select=id,nombre,descripcion,dificultad&order=id.asc";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error cargando tareas: " + request.error);
            yield break;
        }

        string json = "{\"tareas\":" + request.downloadHandler.text + "}";
        TareaLista lista = JsonUtility.FromJson<TareaLista>(json);
        tareas = lista.tareas;

        indiceActual = 0;
        MostrarTareaActual();
        StartCoroutine(CargarLeaderboard(tareas[indiceActual].id)); // Cargar leaderboard de tarea 1 al iniciar
    }

    IEnumerator CargarLeaderboard(int tareaId)
    {
        string orden = (tareaId == 3) ? "precision.asc" : "precision.desc";

        string url = $"{supabaseUrl}/rest/v1/resultados_tareas?select=usuario_id,usuarios(nombre),precision,tiempo,tarea_id&order={orden}&tarea_id=eq.{tareaId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error cargando leaderboard: " + request.error);
            yield break;
        }

        string json = "{\"items\":" + request.downloadHandler.text + "}";
        LeaderboardLista lista = JsonUtility.FromJson<LeaderboardLista>(json);

        Dictionary<string, LeaderboardEntry> mejoresPorUsuario = new();

        foreach (var entry in lista.items)
        {
            if (!mejoresPorUsuario.ContainsKey(entry.usuario_id))
            {
                mejoresPorUsuario[entry.usuario_id] = entry;
            }
            else
            {
                var mejor = mejoresPorUsuario[entry.usuario_id];

                // Dependiendo de la tarea, elegir mejor precisión (o peor si es tarea 3)
                if (tareaId == 3)
                {
                    // Para tarea 3: precisión más alta gana
                    if (entry.precision > mejor.precision)
                        mejoresPorUsuario[entry.usuario_id] = entry;
                }
                else
                {
                    // Para tarea 1 y 2: precisión más baja gana
                    if (entry.precision < mejor.precision)
                        mejoresPorUsuario[entry.usuario_id] = entry;
                }
            }
        }

        foreach (Transform child in leaderboardContent)
            Destroy(child.gameObject);
        
        var ordenados = (tareaId == 2)
            ? mejoresPorUsuario.Values.OrderByDescending(e => e.precision)
            : mejoresPorUsuario.Values.OrderBy(e => e.precision);

        foreach (var entry in ordenados)
        {
            GameObject item = Instantiate(leaderboardItemPrefab, leaderboardContent);
            TMP_Text[] textos = item.GetComponentsInChildren<TMP_Text>();
            textos[0].text = entry.usuarios.nombre;

            if (tareaId == 1)
                textos[1].text = "Completado";
            else if (tareaId == 3)
                textos[1].text = $"{entry.precision:F0}";
            else
                textos[1].text = $"{entry.precision:F2}";
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
    public class LeaderboardEntry
    {
        public string usuario_id;
        public Usuario usuarios;
        public float precision;
        public float tiempo;
        public int tarea_id;
    }

    [System.Serializable]
    public class Usuario
    {
        public string nombre;
    }

    [System.Serializable]
    public class LeaderboardLista
    {
        public LeaderboardEntry[] items;
    }
}
