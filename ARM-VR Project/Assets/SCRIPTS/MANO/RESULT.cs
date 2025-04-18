using UnityEngine;
using UnityEngine.UI;

public class ResultadosUI : MonoBehaviour
{
    public Text resultadoTexto;

    public void MostrarResultado(float tiempo, float precision)
    {
        resultadoTexto.text = $"⏱ Tiempo: {tiempo:F2}s\n🎯 Precisión: {precision:F3} unidades";
    }
}
