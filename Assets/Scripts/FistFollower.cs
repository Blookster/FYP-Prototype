using UnityEngine;

public class FistFollower : MonoBehaviour
{
    [Header("Target VR Controller")]
    public Transform controllerTransform;

    [Header("Locked Fist Scale")]
    public Vector3 customScale = new Vector3(0.01f, 0.01f, 0.01f); // Change this to your preferred size

    void LateUpdate()
    {
        // 1. Only follow position (rotation and scale are completely ignored)
        if (controllerTransform != null)
        {
            transform.position = controllerTransform.position;
        }

        // 2. Hard-lock the scale every frame so nothing can force it to 1.5
        transform.localScale = customScale;
    }
}