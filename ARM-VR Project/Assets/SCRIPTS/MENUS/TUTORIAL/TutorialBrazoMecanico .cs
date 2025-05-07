using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TutorialBrazoMecanico : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private V0HANDCONTROL controlBrazo;
    [SerializeField] private Image imagenAyuda;
    [SerializeField] private GameObject objetoParaAgarrar;

    [Header("UI Tutorial")]
    [SerializeField] private GameObject panelTutorial;
    [SerializeField] private TextMeshProUGUI instruccionesTMP;
    [SerializeField] private Sprite[] spritesInstrucciones;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonFinalizar;
    [SerializeField] private Button botonComenzarPractica; // Nuevo botón
    [SerializeField] private TextMeshProUGUI mensajeFinalTMP;
    [SerializeField] private TextMeshProUGUI debugTMP;

    [Header("Configuración")]
    [SerializeField] private bool permitirPractica = false;

    private int pasoActual = 0;

    private void Start()
    {
        // Configurar botones
        botonSiguiente.onClick.AddListener(SiguientePaso);
        botonFinalizar.onClick.AddListener(FinalizarTutorial);
        botonComenzarPractica.onClick.AddListener(ComenzarPractica);
        
        // Estado inicial
        controlBrazo.enabled = true;
        botonComenzarPractica.gameObject.SetActive(false);
        mensajeFinalTMP.text = "";
        debugTMP.text = "";

        MostrarPanelInicial();
    }

    private void MostrarPanelInicial()
    {
        panelTutorial.SetActive(true);
        instruccionesTMP.text = "Bienvenido al tutorial del brazo mecánico.\n\nAprenderás los controles básicos para manipular el brazo.";
        //imagenAyuda.sprite = spritesInstrucciones[0];
        botonSiguiente.gameObject.SetActive(true);
        botonFinalizar.gameObject.SetActive(false);
        botonComenzarPractica.gameObject.SetActive(false);
        pasoActual = 0;
    }

    private void SiguientePaso()
    {
        pasoActual++;
        
        switch(pasoActual)
        {
            case 1:
                MostrarInstruccion(
                    "1. Movimiento de Base\nUsa [W/A/S/D] para mover la base del brazo"//,
                    //spritesInstrucciones[1]
                );
                break;
            case 2:
                MostrarInstruccion(
                    "2. Movimiento del Antebrazo\nUsa [Q/E] para mover el antebrazo"//,
                    //spritesInstrucciones[2]
                );
                break;
            case 3:
                MostrarInstruccion(
                    "3. Rotación de la Pinza\nUsa [FLECHAS] para rotar la pinza"//,
                    //spritesInstrucciones[3]
                );
                break;
            case 4:
                MostrarInstruccion(
                    "4. Control de la Pinza\nUsa [SPACE] para abrir/cerrar la pinza"//,
                    //spritesInstrucciones[4]
                );
                botonSiguiente.gameObject.SetActive(false);
                
                if(permitirPractica)
                {
                    botonComenzarPractica.gameObject.SetActive(true);
                }
                else
                {
                    botonFinalizar.gameObject.SetActive(true);
                }
                break;
        }
    }

    private void MostrarInstruccion(string texto)//, Sprite imagen)
    {
        instruccionesTMP.text = texto;
        //imagenAyuda.sprite = imagen;
    }

    private void ComenzarPractica()
    {
        instruccionesTMP.text = "¡Ahora practica con los controles!";
        //imagenAyuda.sprite = spritesInstrucciones[5];
        botonComenzarPractica.gameObject.SetActive(false);
        botonFinalizar.gameObject.SetActive(true);
                
        if(objetoParaAgarrar != null)
        {
            objetoParaAgarrar.SetActive(true);
        }
    }

    private void FinalizarTutorial()
    {
        panelTutorial.SetActive(false);
        controlBrazo.enabled = true;
        
        mensajeFinalTMP.text = "Tutorial completado correctamente";
        debugTMP.text = "";

        Debug.Log("Tutorial completado");
        var evaluador = Object.FindFirstObjectByType<EvaluadorTutorial>();
        if (evaluador != null)
        {
            evaluador.RegistrarTutorial(mensajeFinalTMP, debugTMP);
        }
        else
        {
            debugTMP.text = "No se encontró EvaluadorTutorial en la escena.";
        }

    }

    // Eliminado el Update ya que no se necesita temporizador
}