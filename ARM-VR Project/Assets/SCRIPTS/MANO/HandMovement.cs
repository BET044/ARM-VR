using UnityEngine;

public class HandMovement : MonoBehaviour
{
    public HingeJoint[] boneJoints = new HingeJoint[5];

    void Start()
    {
        foreach (var joint in boneJoints)
        {
            if (joint != null)
            {
                joint.useMotor = true;
            }
        }
    }

    public void RotateBone(int boneIndex, float speed)
    {
        if (boneIndex < 0 || boneIndex >= boneJoints.Length || boneJoints[boneIndex] == null)
        {
            Debug.LogWarning("Índice de hueso fuera de rango o HingeJoint no asignado.");
            return;
        }

        JointMotor motor = boneJoints[boneIndex].motor;
        motor.force = 1000f;  // Ajusta la fuerza del motor según sea necesario.
        motor.targetVelocity = speed;
        boneJoints[boneIndex].motor = motor;
    }

    public void StopBoneRotation(int boneIndex)
    {
        if (boneIndex < 0 || boneIndex >= boneJoints.Length || boneJoints[boneIndex] == null)
        {
            return;
        }

        JointMotor motor = boneJoints[boneIndex].motor;
        motor.targetVelocity = 0;
        boneJoints[boneIndex].motor = motor;
    }

    public void ResetHand()
    {
        for (int i = 0; i < boneJoints.Length; i++)
        {
            StopBoneRotation(i);
        }
    }
}
