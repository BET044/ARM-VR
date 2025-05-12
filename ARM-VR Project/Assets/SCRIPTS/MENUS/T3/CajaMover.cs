using UnityEngine;

public class CajaMover : MonoBehaviour
{
    public float velocidad = 2f;
    private bool sobreCinta = false;

    void Update()
    {
        if (sobreCinta)
        {
            transform.position += Vector3.right * velocidad * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZonaCinta"))
        {
            sobreCinta = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ZonaCinta"))
        {
            sobreCinta = false;
        }
    }
}
