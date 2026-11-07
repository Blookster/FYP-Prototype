using System.Collections;
using UnityEngine;

public class ATP_Script : MonoBehaviour
{
    public string atpTypeName;

    [SerializeField]
    private bool isActive;
    private bool isConsumed = false;

    public float activeDelay = 0.1f;

    [Header("Detection Tuning")]
    [Tooltip("Max angle difference (in degrees) allowed between punch velocity and required direction. 65-75 feels responsive in VR.")]
    public float maxAllowedAngle = 70.0f;
    public float minPunchSpeed = 0.8f;

    private void Start()
    {
        isActive = false;
        isConsumed = false;
        StartCoroutine(I_WaitToActive());
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDetectPunch(other);
    }

    // Catches punches if the glove was already overlapping when activeDelay finished
    private void OnTriggerStay(Collider other)
    {
        TryDetectPunch(other);
    }

    private void TryDetectPunch(Collider other)
    {
        if (!isActive || isConsumed) return;
        if (!other.CompareTag("PlayerHand")) return;

        // Attach or grab HandVelocityTracker for smoothed VR velocity + direction
        HandVelocityTracker tracker = other.GetComponentInParent<HandVelocityTracker>();
        if (tracker == null)
        {
            tracker = other.gameObject.AddComponent<HandVelocityTracker>();
        }

        // Fallback to PlayerHandSpeedDetecter if tracker just attached this frame
        PlayerHandSpeedDetecter speedDetector = other.GetComponent<PlayerHandSpeedDetecter>();
        float speed = tracker.IsReady ? tracker.Speed : (speedDetector != null ? speedDetector.currentSpeed : 0f);

        if (speed < minPunchSpeed) return;

        // Calculate actual movement direction of the glove (or entry vector on frame 1)
        Vector3 punchDir = tracker.IsReady && tracker.Velocity.sqrMagnitude > 0.001f
            ? tracker.Velocity.normalized
            : (transform.position - other.transform.position).normalized;

        // Expected punch travel direction based on ATP type
        Vector3 requiredDir;
        switch (atpTypeName)
        {
            case "F_ATP": // Straight punch into the target (+forward)
                requiredDir = transform.forward;
                break;
            case "L_ATP": // Punch coming from the left side moving right (+right)
                requiredDir = transform.right;
                break;
            case "R_ATP": // Punch coming from the right side moving left (-right)
                requiredDir = -transform.right;
                break;
            case "B_ATP": // Uppercut coming from the bottom moving up (+up)
                requiredDir = transform.up;
                break;
            default:
                Debug.LogWarning("Type of ATP not found: " + atpTypeName);
                return;
        }

        float angle = Vector3.Angle(requiredDir, punchDir);

        if (angle <= maxAllowedAngle)
        {
            isConsumed = true;
            DetectionHit();
        }
        else
        {
            DetectionMiss();
        }
    }

    void DetectionHit()
    {
        GameManager.instance.audioManager.Play(0, "hitSfx", false);
        AttackPointManager.instance.DestroyCurrentATP();
        Debug.Log("Hit!!!");
    }

    void DetectionMiss()
    {
        GameManager.instance.audioManager.Play(0, "missSfx", false);
        Debug.Log("Miss!!!");
    }

    IEnumerator I_WaitToActive()
    {
        yield return new WaitForSeconds(activeDelay);
        isActive = true;
    }
}