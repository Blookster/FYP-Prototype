using UnityEngine;

public class FistFollower : MonoBehaviour
{
    [Header("Target VR Controller")]
    public Transform controllerTransform;

    [Header("Locked Fist Scale")]
    public Vector3 customScale = new Vector3(0.01f, 0.01f, 0.01f); // Change this to your preferred size

    [Header("Fist Rotation Offset")]
    public Quaternion rotOffset = new Quaternion(180.0f, 0.0f, 0.0f, 0.0f);


    void LateUpdate()
    {
        if (controllerTransform == null)
            return;

        // Match the controller's position.
        transform.position = controllerTransform.position;

        // Match the controller's rotation.
        transform.rotation = controllerTransform.rotation * rotOffset;

        // Keep the configured glove scale.
        transform.localScale = customScale;
    }

}