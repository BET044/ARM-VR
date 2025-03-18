using UnityEngine;
using UnityEngine.InputSystem;


public class HandControlInputSystem : MonoBehaviour
{
    public HandMovement handMovement;
    public InputActionAsset inputActionsAsset;

    private InputAction[] boneActions = new InputAction[5];
    private InputAction resetAction;
    private InputActionMap handMap;

    public float rotationSpeed = 90f;

    void Awake()
    {
        handMap = inputActionsAsset.FindActionMap("HandMap", true);
        for (int i = 0; i < 5; i++)
        {
            boneActions[i] = handMap.FindAction($"Bone{i}", true);
        }
        resetAction = handMap.FindAction("Reset", true);
    }

    void OnEnable() => handMap.Enable();
    void OnDisable() => handMap.Disable();

    void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            ApplyRotation(i, boneActions[i].ReadValue<float>());
        }

        if (resetAction.triggered)
        {
            handMovement.ResetHand();
        }
    }

    private void ApplyRotation(int boneIndex, float input)
    {
        if (Mathf.Abs(input) > 0.1f)
        {
            float speed = input * rotationSpeed;
            handMovement.RotateBone(boneIndex, speed);
        }
        else
        {
            handMovement.StopBoneRotation(boneIndex);
        }
    }
}
