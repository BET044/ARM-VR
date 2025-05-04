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
    public TMP_Text[] historicoTextos; 
    public TMP_Text[] dificultadTextos;

    private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";


    void Start()
    {
        StartCoroutine(CargarTareas());
    }

    IEnumerator CargarTareas()
    {
        bool esInvitado = !PlayerPrefs.HasKey("user_id") || string.IsNullOrEmpty(PlayerPrefs.GetString("user_id"));

        string urlTareas = $"{supabaseUrl}/rest/v1/tareas?select=id,nombre,descripcion,dificultad&limit=3&order=id.asc";
        UnityWebRequest requestTareas = UnityWebRequest.Get(urlTareas);
        requestTareas.SetRequestHeader("apikey", apiKey);
        
        if (!esInvitado)
        {
            requestTareas.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        }

        yield return requestTareas.SendWebRequest();

        if (requestTareas.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error al cargar tareas: " + requestTareas.error);
            yield break;
        }

        string jsonTareas = "{\"tareas\":" + requestTareas.downloadHandler.text + "}";
        TareaLista listaTareas = JsonUtility.FromJson<TareaLista>(jsonTareas);

        Dictionary<int, TareaHistorico> tareasCompletadasHistorico = new Dictionary<int, TareaHistorico>();
        
        if (!esInvitado)
        {
            string usuarioId = PlayerPrefs.GetString("user_id");
            string urlCompletadasHistorico = $"{supabaseUrl}/rest/v1/rpc/obtener_tareas_completadas?usuario_id={usuarioId}";
            UnityWebRequest requestCompletadasHistorico = UnityWebRequest.Get(urlCompletadasHistorico);
            requestCompletadasHistorico.SetRequestHeader("apikey", apiKey);
            requestCompletadasHistorico.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
            requestCompletadasHistorico.SetRequestHeader("Content-Type", "application/json");

            yield return requestCompletadasHistorico.SendWebRequest();

            if (requestCompletadasHistorico.result == UnityWebRequest.Result.Success)
            {
                string jsonHistorico = requestCompletadasHistorico.downloadHandler.text;
                TareaHistoricoLista historico = JsonUtility.FromJson<TareaHistoricoLista>("{\"items\":" + jsonHistorico + "}");

                foreach (var item in historico.items)
                {
                    if (!tareasCompletadasHistorico.ContainsKey(item.tarea_id))
                    {
                        tareasCompletadasHistorico.Add(item.tarea_id, item);
                    }
                }
            }
        }

        // Mostrar información en la UI
        for (int i = 0; i < listaTareas.tareas.Length && i < 3; i++)
        {
            int tareaId = listaTareas.tareas[i].id;
            
            // Mostrar información básica
            tituloTextos[i].text = listaTareas.tareas[i].nombre;
            descripcionTextos[i].text = listaTareas.tareas[i].descripcion;
            
            // Mostrar dificultad con color
            if (dificultadTextos != null && i < dificultadTextos.Length)
            {
                dificultadTextos[i].text = ObtenerTextoDificultad(listaTareas.tareas[i].dificultad);
            }
            
            // Mostrar historial o mensaje para invitados
            if (esInvitado)
            {
                historicoTextos[i].text = "Inicia sesión para ver tu progreso";
            }
            else if (tareasCompletadasHistorico.TryGetValue(tareaId, out TareaHistorico historico))
            {
                historicoTextos[i].text = $"Completada {historico.veces_completada} veces\n" +
                                         $"Mejor tiempo: {historico.mejor_tiempo}s\n" +
                                         $"Menor error: {historico.mejor_precision}%";
            }
            else
            {
                historicoTextos[i].text = "Nunca completada";
            }
        }
    }

    // Función para formatear el texto de dificultad con colores
    private string ObtenerTextoDificultad(string dificultad)
    {
        if (string.IsNullOrEmpty(dificultad))
        {
            return "<color=#808080>No especificada</color>"; // Gris
        }

        switch (dificultad.ToLower())
        {
            case "Tutorial":
                return "<color=#2196F3>Fácil</color>"; //  Azul
            case "media":
                return "<color=#4CAF50>Media</color>"; //  Verde
            case "dificil":
            case "difícil":
                return "<color=#FFC107>Difícil</color>"; //  Amarillo
            case "experto":
                return "<color=#F44336>Experto</color>"; //  Rojo
            default:
                return $"<color=#9C27B0>{dificultad}</color>"; //  para valores personalizados Morado
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
    public class TareaHistorico
    {
        public int tarea_id;
        public int veces_completada;
        public float mejor_tiempo;
        public float mejor_precision;
    }

    [System.Serializable]
    public class TareaHistoricoLista
    {
        public TareaHistorico[] items;
    }
}