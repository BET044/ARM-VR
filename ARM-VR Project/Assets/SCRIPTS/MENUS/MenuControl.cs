using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class MenuControl : MonoBehaviour
{
   
    public string escena1 = "escena1";
    public string escena2 = "escena2";
    public string escena3 = "escena3";
    // Supabase
    //private string supabaseUrl = "https://hgnrgwruwxkdhhrpguou.supabase.co"; 
    //private string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhnbnJnd3J1d3hrZGhocnBndW91Iiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM0NDQ0NDksImV4cCI6MjA1OTAyMDQ0OX0.WtiXzBIdQORbOWzOs3zBQgHR6Yr7MnC-q6ihZ1OT5fw";

    void Start()
    {

    }

    public void Boton1()
    {
        SceneManager.LoadScene(escena1);
    }

    public void Boton2()
    {
        SceneManager.LoadScene(escena2);
    }

    public void Boton3()
    {
        SceneManager.LoadScene(escena3);
    }

    public void BotonSalir()
    {
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("user_id");
        PlayerPrefs.DeleteKey("user_email");
        PlayerPrefs.DeleteKey("session_data");

        Debug.Log("Sesión cerrada.");
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}