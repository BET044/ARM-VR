using UnityEngine;

public class DestruirCaja : MonoBehaviour
{
    [Header("Opciones de conteo")]
    [SerializeField] private bool esContador = false;  // Checkbox en el inspector

    public int cajasEntregadas { get; private set; } = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Objeto"))
        {
            Destroy(other.gameObject);

            if (esContador)
            {
                cajasEntregadas++;
                Debug.Log("Caja entregada. Total: " + cajasEntregadas);
            }
        }
    }
}
