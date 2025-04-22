using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.SceneManagement;

public class V0HANDCONTROL : MonoBehaviour
{
    // References--------------------------------------------------------------------------------------------------------------
    [SerializeField] private EvaluadorTarea evaluadorTarea;
    [SerializeField] private Transform objetivoFinal;

    [SerializeField] private V0HANDMOVE _handMove;
    [SerializeField] private InputActionAsset _inputActionsAsset;
    [SerializeField] private Transform _puntoDeAgarre;
    [SerializeField] private Transform _puntaDeLaGarra;

    [Header("Raycast Settings")]
    [Tooltip("Dirección del raycast relativa a la punta de la garra")]
    [SerializeField] private Vector3 _direccionRayo = Vector3.forward;

    [Tooltip("Distancia máxima del raycast")]
    [SerializeField] private float _distanciaRango = 1.0f;

    [Tooltip("LayerMask para filtrar objetos")]
    [SerializeField] private LayerMask _layerMask = ~0;

    [Tooltip("Velocidad de rotación de los dedos")]
    [SerializeField] private float _rotationSpeed = 90f;

    // Input Actions-----------------------------------------------------------------------------------------------------------
    private InputAction _bone0, _bone1, _bone2, _bone3, _bone4, _resetAction, _agarrarAction;
    private InputActionMap _handMap;

    // State -----------------------------------------------------------------------------------------------------------
    private float[] _boneAngles = new float[5];
    private bool _agarrando = false;
    private GameObject _objetoAgarrado;
    private Collider[] _collidersMano;

    public bool Agarrando => _agarrando;

    ///-----------------------------------------------------------------------------------
    private void Awake()
    {
        ValidateReferences();
        _collidersMano = GetComponentsInChildren<Collider>();
        SetupInputActions();
        // Resetear la mano al iniciar
        ResetHandImmediately();
        
    }


    
private void ResetHandImmediately()
{
    _handMove?.ResetHand();
    _boneAngles = new float[5];
}

    private void ValidateReferences()
    {
        Debug.Assert(_handMove != null, "V0HANDMOVE reference is not assigned!", this);
        Debug.Assert(_inputActionsAsset != null, "InputActionAsset is not assigned!", this);
        Debug.Assert(_puntoDeAgarre != null, "PuntoDeAgarre transform is not assigned!", this);
        Debug.Assert(_puntaDeLaGarra != null, "PuntaDeLaGarra transform is not assigned!", this);
    }

    private void SetupInputActions()
    {
        _handMap = _inputActionsAsset.FindActionMap("HandMap");
        Debug.Assert(_handMap != null, "Could not find HandMap in InputActionAsset!", this);

        _bone0 = _handMap.FindAction("Bone0");
        _bone1 = _handMap.FindAction("Bone1");
        _bone2 = _handMap.FindAction("Bone2");
        _bone3 = _handMap.FindAction("Bone3");
        _bone4 = _handMap.FindAction("Bone4");
        _resetAction = _handMap.FindAction("Reset");
        _agarrarAction = _handMap.FindAction("Agarrar");
    }

    private void OnEnable() => _handMap?.Enable();

    private void OnDisable()
    {
        _handMap?.Disable();
        if (_agarrando) SoltarObjeto();
    }

    private void Update()
    {
        DrawDebugRay();
        ProcessBoneRotations();
        HandleResetAction();
        HandleGrabAction();
    }

    private void DrawDebugRay()
    {
        if (_puntaDeLaGarra == null) return;

        Vector3 dir = _puntaDeLaGarra.TransformDirection(_direccionRayo.normalized);
        Debug.DrawRay(_puntaDeLaGarra.position, dir * _distanciaRango, Color.green);
    }

    private void ProcessBoneRotations()
    {
        ProcessSingleBoneRotation(_bone0, 0);
        ProcessSingleBoneRotation(_bone1, 1);
        ProcessSingleBoneRotation(_bone2, 2);
        ProcessSingleBoneRotation(_bone3, 3);
        ProcessSingleBoneRotation(_bone4, 4);
    }

    private void ProcessSingleBoneRotation(InputAction action, int boneIndex)
    {
        if (action == null) return;

        float input = action.ReadValue<float>();
        if (Mathf.Abs(input) > 0.1f)
        {
            _boneAngles[boneIndex] += input * _rotationSpeed * Time.deltaTime;
            _handMove.RotateBone(boneIndex, _boneAngles[boneIndex]);
        }
    }

    private void HandleResetAction()
    {
        if (_resetAction?.triggered ?? false)
        {
            _handMove.ResetHand();
            _boneAngles = new float[5];
            SceneManager.LoadScene("ARM ESCENE");
        }
    }

    private void HandleGrabAction()
    {
        if (!(_agarrarAction?.triggered ?? false)) return;

        if (_agarrando)
            SoltarObjeto();
        else
            IntentarAgarrarObjetoConRaycast();
    }

    private void IntentarAgarrarObjetoConRaycast()
    {
        if (_puntaDeLaGarra == null) return;

        Vector3 origen = _puntaDeLaGarra.position;
        Vector3 direccion = _puntaDeLaGarra.TransformDirection(_direccionRayo.normalized);

        var hit = FindNearestGrabbableObject(origen, direccion);
        if (hit.HasValue) // Verificar si hay un hit válido
        {
            AgarrarObjeto(hit.Value.collider.gameObject); // Acceder al Value del nullable
        }
        else
        {
            Debug.Log("No se detectó ningún objeto agarrable");
        }
    }

    private RaycastHit? FindNearestGrabbableObject(Vector3 origin, Vector3 direction)
    {
        // Usamos RaycastAll ordenado por distancia como primera opción
        var hits = Physics.RaycastAll(origin, direction, _distanciaRango, _layerMask)
            .Where(h => !_collidersMano.Contains(h.collider) && h.collider.CompareTag("Objeto"))
            .OrderBy(h => h.distance)
            .FirstOrDefault();

        return hits.collider != null ? hits : (RaycastHit?)null;
    }
    private void AgarrarObjeto(GameObject objeto)
    {
        if (objeto == null || _puntoDeAgarre == null) return;

        // Desactivar completamente la física del objeto
        if (objeto.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None; // Cambiado a None para mejor sincronización
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }

        // Desactivar colisiones temporales
        foreach (var collider in objeto.GetComponents<Collider>())
        {
            collider.enabled = false;
        }

        _objetoAgarrado = objeto;

        // Configurar la transformación del objeto
        _objetoAgarrado.transform.SetParent(_puntoDeAgarre);
        _objetoAgarrado.transform.localPosition = Vector3.zero;
        _objetoAgarrado.transform.localRotation = Quaternion.identity;

        // Forzar una actualización inmediata
        _objetoAgarrado.transform.hasChanged = true;

        _agarrando = true;
        Debug.Log($"Objeto {objeto.name} agarrado correctamente");
        evaluadorTarea?.IniciarTarea(objeto, objetivoFinal);
    }

    private void SoltarObjeto()
    {
        if (_objetoAgarrado == null) return;
        evaluadorTarea?.FinalizarTarea();

        // Restaurar configuración física
        if (_objetoAgarrado.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        // Reactivar colisiones
        foreach (var collider in _objetoAgarrado.GetComponents<Collider>())
        {
            collider.enabled = true;
        }

        _objetoAgarrado.transform.SetParent(null);
        _agarrando = false;
        _objetoAgarrado = null;

        Debug.Log("Objeto soltado correctamente");
    }

    private void LateUpdate()
    {
        if (_agarrando && _objetoAgarrado != null)
        {
            // Forzar la actualización de la posición en cada frame
            _objetoAgarrado.transform.position = _puntoDeAgarre.position;
            _objetoAgarrado.transform.rotation = _puntoDeAgarre.rotation;
        }
    }

    private void ConfigureRigidbody(GameObject obj, bool isKinematic)
    {
        if (obj.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = isKinematic;
            if (isKinematic)
            {
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}