using UnityEngine;

public class PlayerHandSpeedDetecter : MonoBehaviour
{
    public float currentSpeed;
    public Vector3 currentVelocity;

    [Range(0.05f, 1.0f)]
    public float smoothing = 0.5f;

    private Vector3 lastPos;

    private void Start()
    {
        GetLastPos();
    }

    void GetLastPos()
    {
        lastPos = transform.position;
    }

    private void LateUpdate()
    {
        if (Time.deltaTime <= 0.0f) return;

        Vector3 instantVelocity = (transform.position - lastPos) / Time.deltaTime;
        currentVelocity = Vector3.Lerp(currentVelocity, instantVelocity, smoothing);
        currentSpeed = currentVelocity.magnitude;

        lastPos = transform.position;
    }
}
