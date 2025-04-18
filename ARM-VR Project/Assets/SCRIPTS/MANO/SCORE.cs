using UnityEngine;
using TMPro;

public class EvaluadorTarea : MonoBehaviour
{
    private float tiempoInicio;
    private GameObject objeto;
    private Transform objetivoFinal;
    private bool tareaActiva = false;

    [Header("UI TMP")]
    [SerializeField] private TextMeshProUGUI tiempoTMP;
    [SerializeField] private TextMeshProUGUI resultadoTMP;

    [Header("Límite de tiempo opcional")]
    [SerializeField] private float tiempoLimite = 10f;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorAlerta = Color.red;

    void Update()
    {
        if (tareaActiva && tiempoTMP != null)
        {
            float tiempoActual = Time.time - tiempoInicio;
            tiempoTMP.text = $"⏱ {tiempoActual:F2}s";

            if (tiempoActual > tiempoLimite)
                tiempoTMP.color = colorAlerta;
            else
                tiempoTMP.color = colorNormal;
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
            tiempoTMP.text = "⏱ 0.00s";
            tiempoTMP.color = colorNormal;
        }

        Debug.Log("Tarea iniciada.");
    }

    public void FinalizarTarea()
    {
        if (!tareaActiva || objeto == null || objetivoFinal == null) return;

        float tiempoTotal = Time.time - tiempoInicio;
        float precision = Vector3.Distance(objeto.transform.position, objetivoFinal.position);

        if (resultadoTMP != null)
        {
            resultadoTMP.text = $"✅ Tiempo: {tiempoTotal:F2}s\n🎯 Precisión: {precision:F3}u";
        }

        Debug.Log($"Tarea finalizada.\nTiempo: {tiempoTotal:F2}s\nPrecisión: {precision:F3} unidades");

        tareaActiva = false;
        objeto = null;
        objetivoFinal = null;
    }
}
