using UnityEngine;

public class HandControl : MonoBehaviour
{
    // Referencia al script de movimiento; así puedes invocar sus métodos.
    public HandMovement handMovement;

    // Ángulos actuales de cada hueso, para poder modificarlos progresivamente.
    private float[] boneAngles = new float[5];
    public float sensibilidad;

    void Update()
    {
        // Ejemplo de control con las teclas numéricas.
        // Cada pulsación aumenta el ángulo del hueso correspondiente en 10 grados.


        
        /// bones[boneIndex].localRotation = Quaternion.Euler(angleX, angleY, angleZ);
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            boneAngles[0] += sensibilidad;
            handMovement.RotateBone(0, boneAngles[0]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            boneAngles[1] += sensibilidad;
            handMovement.RotateBone(1, boneAngles[1]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            boneAngles[2] += sensibilidad;
            handMovement.RotateBone(2, boneAngles[2]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            boneAngles[3] += sensibilidad;
            handMovement.RotateBone(3, boneAngles[3]);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            boneAngles[4] += sensibilidad;
            handMovement.RotateBone(4, boneAngles[4]);
        }
        
        // Pulsando 'R' se resetea la mano a su posición inicial.
        if (Input.GetKeyDown(KeyCode.R))
        {
            handMovement.ResetHand();
            boneAngles = new float[5];
        }
    }
}
