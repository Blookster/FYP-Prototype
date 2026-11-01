using UnityEngine;

public class PlayerHandSpeedDetecter : MonoBehaviour
{
    public float currentSpeed;
    Vector3 lastPos;

    private void Start()
    {
        GetLastPos();
    }

    void GetLastPos()
    {
        lastPos = transform.position;
    }

    private void Update()
    {
        float distanceMoved = Vector3.Distance(lastPos, transform.position);
        currentSpeed = distanceMoved / Time.deltaTime;
        lastPos = transform.position;
    }
}
