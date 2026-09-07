using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public enum FighterState
    {
        Idle,
        Attack,
        Blocking,
        Dodge,
        BeenHit
    }

    [Header("Player Stats")]
    public float playerHP = 100f;
    public float playerJumpHeight = 5f;
    public FighterState currentState;

    [Header("Hand References")]
    public Transform leftHand;
    public Transform rightHand;

    [Header("Special Weapon Power Settings")]
    public bool isSpecialActive = false;
    public float specialDuration = 10f;
    private float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f; 

    private Rigidbody rb;

    void Start()
    {
        currentState = FighterState.Idle;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Handle special weapon timer
        if (isSpecialActive)
        {
            specialTimer -= Time.deltaTime;
            if (specialTimer <= 0)
            {
                DeactivateSpecialPower();
            }
        }

        // Check for Fist-Bump Special Activation
        CheckFistBump();

        // Check for Ground Slam Jump
        CheckGroundSlamJump();
    }

    void CheckFistBump()
    {
        if (!isSpecialActive && leftHand != null && rightHand != null)
        {
            float distance = Vector3.Distance(leftHand.position, rightHand.position);

            // If fists are close together, activate special state!
            if (distance < fistBumpDistanceThreshold)
            {
                ActivateSpecialPower();
            }
        }
    }

    void ActivateSpecialPower() // temporary idea for now might change later
    {
        isSpecialActive = true;
        specialTimer = specialDuration;
        Debug.Log("Special Power Activated! Fists are super-sized/powered.");

        // Scale up hands temporarily as visual feedback
        if (leftHand != null) leftHand.localScale = Vector3.one * 1.5f;
        if (rightHand != null) rightHand.localScale = Vector3.one * 1.5f;
    }

    void DeactivateSpecialPower()
    {
        isSpecialActive = false;
        Debug.Log("Special Power Ended.");

        // Reset hand scale
        if (leftHand != null) leftHand.localScale = Vector3.one;
        if (rightHand != null) rightHand.localScale = Vector3.one;
    }

    void CheckGroundSlamJump()
    {
        // Simple check: if both hands are pushed down low (e.g., Y position below 0.3 relative to the player)
        if (leftHand.position.y < 0.4f && rightHand.position.y < 0.4f)
        {
            if (rb != null)
            {
                // Launch the player upwards
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, playerJumpHeight, rb.linearVelocity.z);
                currentState = FighterState.Dodge;
                Debug.Log("Ground Slam Jump Triggered!");
            }
        }
    }
}