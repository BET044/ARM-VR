using UnityEngine;
using UnityEngine.InputSystem;

public class HandControlInputSystem : MonoBehaviour
{
    // Referencia al script que mueve la mano.
    public HandMovement handMovement;

    // Referencia al Input Action Asset que contiene el action map.
    public InputActionAsset inputActionsAsset;

    // InputActions individuales para cada hueso.
    private InputAction bone0;
    private InputAction bone1;
    private InputAction bone2;
    private InputAction bone3;
    private InputAction bone4;
    private InputAction resetAction;

    // Ángulos actuales para cada hueso.
    private float[] boneAngles = new float[5];

    // Velocidad de rotación en grados por segundo.
    public float rotationSpeed = 90f;

    // Action map configurado (asegúrate de que el nombre coincida con el definido en el asset).
    private InputActionMap handMap;

    void Awake()
    {
        // Obtén el action map llamado "HandMap" desde el asset.
        handMap = inputActionsAsset.FindActionMap("HandMap", true);
        
        // Obtén las acciones configuradas.
        // Cada acción debe configurarse como "Value" de tipo "Axis" (1D) que retorne un float.
        bone0 = handMap.FindAction("Bone0", true);
        bone1 = handMap.FindAction("Bone1", true);
        bone2 = handMap.FindAction("Bone2", true);
        bone3 = handMap.FindAction("Bone3", true);
        bone4 = handMap.FindAction("Bone4", true);
        resetAction = handMap.FindAction("Reset", true);
    }

    void OnEnable()
    {
        handMap.Enable();
    }

    void OnDisable()
    {
        handMap.Disable();
    }

    void Update()
    {
        // Lee el valor del eje para cada hueso.
        float input0 = bone0.ReadValue<float>();
        float input1 = bone1.ReadValue<float>();
        float input2 = bone2.ReadValue<float>();
        float input3 = bone3.ReadValue<float>();
        float input4 = bone4.ReadValue<float>();

        // Usar un umbral para evitar movimientos por "ruido" (por ejemplo, 0.1f).
        if (Mathf.Abs(input0) > 0.1f)
        {
            boneAngles[0] += input0 * rotationSpeed * Time.deltaTime;
            handMovement.RotateBone(0, boneAngles[0]);
        }
        if (Mathf.Abs(input1) > 0.1f)
        {
            boneAngles[1] += input1 * rotationSpeed * Time.deltaTime;
            handMovement.RotateBone(1, boneAngles[1]);
        }
        if (Mathf.Abs(input2) > 0.1f)
        {
            boneAngles[2] += input2 * rotationSpeed * Time.deltaTime;
            handMovement.RotateBone(2, boneAngles[2]);
        }
        if (Mathf.Abs(input3) > 0.1f)
        {
            boneAngles[3] += input3 * rotationSpeed * Time.deltaTime;
            handMovement.RotateBone(3, boneAngles[3]);
        }
        if (Mathf.Abs(input4) > 0.1f)
        {
            boneAngles[4] += input4 * rotationSpeed * Time.deltaTime;
            handMovement.RotateBone(4, boneAngles[4]);
        }

        // Reinicia la mano si se activa la acción "Reset".
        if (resetAction.triggered)
        {
            handMovement.ResetHand();
            boneAngles = new float[5];
        }
    }
}
