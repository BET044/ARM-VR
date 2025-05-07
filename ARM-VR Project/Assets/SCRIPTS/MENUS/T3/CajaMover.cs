using UnityEngine;

public class CajaMover : MonoBehaviour
{
    public float velocidad = 2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Movimiento constante en el eje X (ajusta según la dirección de tu cinta)
        rb.MovePosition(rb.position + Vector3.right * velocidad * Time.fixedDeltaTime);
    }
}
