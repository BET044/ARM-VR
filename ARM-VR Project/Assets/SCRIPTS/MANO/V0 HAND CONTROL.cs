using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq; // Necesario para el OrderBy

public class V0HANDCONTROL : MonoBehaviour
{
    // References
    public V0HANDMOVE V0HANDMOVE;
    public InputActionAsset inputActionsAsset;
    public Transform puntoDeAgarre;
    public Transform puntaDeLaGarra;
    
    [Header("Raycast Settings")]
    [Tooltip("Dirección del raycast relativa a la punta de la garra")]
    public Vector3 direccionRayo = Vector3.forward;
    
    [Tooltip("Distancia máxima del raycast")]
    public float distanciaRango = 1.0f;
    
    [Tooltip("LayerMask para filtrar objetos")]
    public LayerMask layerMask = ~0; // Por defecto detecta todo
    
    [Tooltip("Velocidad de rotación de los dedos")]
    public float rotationSpeed = 90f;
    
    // Input Actions
    private InputAction bone0;
    private InputAction bone1;
    private InputAction bone2;
    private InputAction bone3;
    private InputAction bone4;
    private InputAction resetAction;
    private InputAction agarrarAction;
    private InputActionMap handMap;
    
    // State
    private float[] boneAngles = new float[5];
    public bool agarrando { get; private set; } = false;
    private GameObject objetoAgarrado;
    private Collider[] collidersMano; // Para ignorar auto-colisiones

    void Awake()
    {
        // Validación de referencias
        if (V0HANDMOVE == null) Debug.LogError("V0HANDMOVE reference is not assigned!", this);
        if (inputActionsAsset == null) Debug.LogError("InputActionAsset is not assigned!", this);
        if (puntoDeAgarre == null) Debug.LogError("PuntoDeAgarre transform is not assigned!", this);
        if (puntaDeLaGarra == null) Debug.LogError("PuntaDeLaGarra transform is not assigned!", this);

        // Obtener todos los colliders de la mano/brazo
        collidersMano = GetComponentsInChildren<Collider>();

        // Configuración de inputs
        handMap = inputActionsAsset.FindActionMap("HandMap");
        if (handMap == null)
        {
            Debug.LogError("Could not find HandMap in InputActionAsset!", this);
            return;
        }

        bone0 = handMap.FindAction("Bone0");
        bone1 = handMap.FindAction("Bone1");
        bone2 = handMap.FindAction("Bone2");
        bone3 = handMap.FindAction("Bone3");
        bone4 = handMap.FindAction("Bone4");
        resetAction = handMap.FindAction("Reset");
        agarrarAction = handMap.FindAction("Agarrar");
    }

    void OnEnable() => handMap?.Enable();
    
    void OnDisable()
    {
        handMap?.Disable();
        if (agarrando) SoltarObjeto();
    }

    void Update()
    {
        // Visualización del raycast
        if (puntaDeLaGarra != null)
        {
            Vector3 dir = puntaDeLaGarra.TransformDirection(direccionRayo.normalized);
            Debug.DrawRay(puntaDeLaGarra.position, dir * distanciaRango, Color.green);
        }

        // Procesar rotaciones de huesos
        ProcessBoneRotation(bone0, 0);
        ProcessBoneRotation(bone1, 1);
        ProcessBoneRotation(bone2, 2);
        ProcessBoneRotation(bone3, 3);
        ProcessBoneRotation(bone4, 4);

        // Reset hand position
        if (resetAction?.triggered ?? false)
        {
            V0HANDMOVE.ResetHand();
            boneAngles = new float[5];
        }

        // Handle grab action
        if (agarrarAction?.triggered ?? false)
        {
            if (agarrando)
                SoltarObjeto();
            else
                IntentarAgarrarObjetoConRaycast();
        }
    }

    private void ProcessBoneRotation(InputAction action, int boneIndex)
    {
        if (action == null) return;
        
        float input = action.ReadValue<float>();
        if (Mathf.Abs(input) > 0.1f)
        {
            boneAngles[boneIndex] += input * rotationSpeed * Time.deltaTime;
            V0HANDMOVE.RotateBone(boneIndex, boneAngles[boneIndex]);
        }
    }

    void IntentarAgarrarObjetoConRaycast()
    {
        if (puntaDeLaGarra == null) return;
        
        Vector3 origen = puntaDeLaGarra.position;
        Vector3 direccion = puntaDeLaGarra.TransformDirection(direccionRayo.normalized);

        // Opción 1: RaycastAll ordenado por distancia
        RaycastHit[] hits = Physics.RaycastAll(origen, direccion, distanciaRango, layerMask)
            .OrderBy(h => h.distance)
            .ToArray();

        foreach (var hit in hits)
        {
            // Ignorar colliders de la propia mano
            if (collidersMano.Contains(hit.collider)) continue;
            
            if (hit.collider.CompareTag("Objeto"))
            {
                AgarrarObjeto(hit.collider.gameObject);
                return;
            }
        }

        // Opción 2: Raycast simple (alternativa)
        RaycastHit hitSimple;
        if (Physics.Raycast(origen, direccion, out hitSimple, distanciaRango, layerMask))
        {
            if (!collidersMano.Contains(hitSimple.collider) && hitSimple.collider.CompareTag("Objeto"))
            {
                AgarrarObjeto(hitSimple.collider.gameObject);
                return;
            }
        }

        Debug.Log("No se detectó ningún objeto agarrable");
    }

    void AgarrarObjeto(GameObject objeto)
    {
        if (objeto == null || puntoDeAgarre == null) return;
        
        // Configurar física
        if (objeto.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        objetoAgarrado = objeto;
        objetoAgarrado.transform.SetParent(puntoDeAgarre);
        objetoAgarrado.transform.localPosition = Vector3.zero;
        objetoAgarrado.transform.localRotation = Quaternion.identity;
        
        agarrando = true;
        Debug.Log($"Objeto {objeto.name} agarrado correctamente");
    }

    void SoltarObjeto()
    {
        if (objetoAgarrado == null) return;
        
        // Restaurar física
        if (objetoAgarrado.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
        }
        
        objetoAgarrado.transform.SetParent(null);
        agarrando = false;
        objetoAgarrado = null;
        
        Debug.Log("Objeto soltado correctamente");
    }
}