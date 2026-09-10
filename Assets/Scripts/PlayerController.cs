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

    [Header("Tracking References")]
    public Transform headCamera; // Drag your Main Camera here
    public Transform leftHand;   // Player Hand 
    public Transform rightHand;  

    [Header("Special Weapon Power Settings")]
    public bool isSpecialActive = false;
    public float specialDuration = 10f;
    private float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f;

    [Header("Combat Settings")]
    public float attackExtensionThreshold = 0.4f; // How far forward hands must move to consider it as a punch
    private bool isBlockingInput = false;
    private bool isDodgeInput = false;

    private Rigidbody rb;

    void Start()
    {
        currentState = FighterState.Idle;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdatePlayerInputs();

        // 2. State Machine Logic Loop
        switch (currentState)
        {
            case FighterState.Idle:
                // Hands down: vulnerable to incoming hits
                if (CheckArmExtension())
                {
                    currentState = FighterState.Attack;
                }
                break;

            case FighterState.Attack:
                // Hand is extended outward to punch
                // Hit detection is handled via collision scripts with the enemy
                if (!CheckArmExtension())
                {
                    currentState = FighterState.Idle; // Return to idle when pulling hands back
                }
                break;

            case FighterState.Blocking:
                // Hands up guarding: reduces or negates incoming damage
                if (!isBlockingInput)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.Dodge:
                // Shifted backward/sideways: out of enemy range if timed right
                if (!isDodgeInput)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.BeenHit:
                // Stunned or taking damage after a failed defense/bad dodge
                // Handled externally when an enemy attack collider hits the player
                break;
        }

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

    void UpdatePlayerInputs()
    {
        // Example: Holding a button or raising hands triggers blocking/dodging
        // You can link these to Meta Quest 3 controller buttons (e.g., Grip button for dodge)
        isBlockingInput = Input.GetKey(KeyCode.F); // Replace with XR Input action if needed
        isDodgeInput = Input.GetKey(KeyCode.Space); // Replace with Grip button input

        if (isBlockingInput && currentState != FighterState.BeenHit) // if the the player is holding up both hands then player state is blocking
        {
            currentState = FighterState.Blocking;
        }
        else if (isDodgeInput && currentState != FighterState.BeenHit) // if the player is dodging then player state is dodging
        {
            currentState = FighterState.Dodge;
        }
    }

    bool CheckArmExtension()
    {
        // Check if either hand controller is pushed forward relative to the headset camera
        float leftZ = leftHand.position.z - headCamera.position.z;
        float rightZ = rightHand.position.z - headCamera.position.z;

        return (leftZ > attackExtensionThreshold || rightZ > attackExtensionThreshold);
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

    public void TakeDamage(float damageAmount)
    {
        if (currentState == FighterState.Blocking)
        {
            Debug.Log("Attack blocked! Reduced or zero damage taken.");
            // Apply reduced damage here if desired
            playerHP -= (damageAmount * 0.2f);
        }
        else if (currentState == FighterState.Dodge)
        {
            Debug.Log("Dodged successfully! Out of range, zero damage.");
        }
        else
        {
            // Vulnerable in Idle or bad dodge timing -> Full damage + BeenHit state
            currentState = FighterState.BeenHit;
            playerHP -= damageAmount;
            Debug.Log("Hit! Player HP remaining: " + playerHP);

            if (playerHP <= 0)
            {
                Debug.Log("Player Knocked Out (KO)!");
            }
        }
    }
}