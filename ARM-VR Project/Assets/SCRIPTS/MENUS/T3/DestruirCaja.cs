using UnityEngine;

public class DestruirCaja : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Objeto"))
        {
            Destroy(other.gameObject);
        }
    }
}
