using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    // Nombres de las escenas (configurables desde el Inspector)
    public string escenaNiveles = "Niveles";
    public string escenaOpciones = "Opciones";
    public string escenaPerfil = "Perfil";

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
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Debug.log("SALIENDO")
        Application.Quit();
        #endif
    }
}