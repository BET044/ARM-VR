using UnityEngine;

public class HANDGRAB : MonoBehaviour
{
    private GameObject objetoAgarrado;
    public Transform puntoDeAgarre; // Un Empty GameObject en la pinza donde se sujetará el objeto.
    private bool agarrando = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Objeto") && !agarrando)
        {
            objetoAgarrado = other.gameObject;
        }
    }

    void Agarrar()
    {
        if (objetoAgarrado != null)
        {
            objetoAgarrado.transform.SetParent(puntoDeAgarre);
            objetoAgarrado.transform.localPosition = Vector3.zero; // Alinear con la pinza
            objetoAgarrado.transform.localRotation = Quaternion.identity;
            Rigidbody rb = objetoAgarrado.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            agarrando = true;
        }
    }
    void Soltar()
    {
        if (objetoAgarrado != null)
        {
            objetoAgarrado.transform.SetParent(null);
            Rigidbody rb = objetoAgarrado.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.AddForce(transform.forward * 2, ForceMode.Impulse); // Opcional: empujar al soltar
            objetoAgarrado = null;
            agarrando = false;
        }
    }


}
