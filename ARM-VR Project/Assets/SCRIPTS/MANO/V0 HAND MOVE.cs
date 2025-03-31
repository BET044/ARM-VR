using UnityEngine;

public class V0HANDMOVE : MonoBehaviour
{
    // Asigna los 5 huesos de la mano en el Inspector.
    public Transform[] bones = new Transform[5];

    /// <summary>
    /// Rota el hueso indicado a un ángulo en el eje X (puedes ajustar el eje según tu modelo).
    /// </summary>
    /// <param name="boneIndex">Índice del hueso (0 a 4).</param>
    /// <param name="angle">Ángulo en grados.</param>
    public void RotateBone(int boneIndex, float angle)
    {
        if (boneIndex < 0 || boneIndex >= bones.Length)
        {
            Debug.LogWarning("Índice de hueso fuera de rango.");
            return;
        }
        
        // Se establece la rotación local para el hueso.
        if ((boneIndex == 3) || (boneIndex == 0))
        {
            // Rotación en el eje Z
            bones[boneIndex].localRotation = Quaternion.Euler(0, angle, 0);
        }
        else
        {
            // Rotación en el eje X para los huesos restantes.
            bones[boneIndex].localRotation = Quaternion.Euler(angle, 0, 0);
        }
    }

    /// <summary>
    /// Restaura la rotación inicial (identidad) de todos los huesos.
    /// </summary>
    public void ResetHand()
    {
        foreach (Transform bone in bones)
        {
            bone.localRotation = Quaternion.identity;
        }
    }
}
