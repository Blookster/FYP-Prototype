using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(FighterVisualManager))]

 // ^ Why it's coded like this: In Unity, scripts often crash with NullReferenceException if a required component is forgotten on the GameObject. These attributes tell Unity: "If you attach PlayerController to a capsule, automatically force-attach these other scripts too." It guarantees structural integrity before the game even presses Play.
 
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

    [Header("State")]
    public FighterState currentState;

    [Header("Tracking References")]
    public Transform headCamera;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Special Weapon Power Settings")]
    public bool isSpecialActive = false;
    public float specialDuration = 10f;
    private float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f;

    [Header("Punch-Out Positioning & Movement")]
    public float dodgeBackDistance = 0.8f;
    public float jumpHeight = 1.5f;
    public float blockHandRaiseY = 0.35f;
    public float attackExtensionThreshold = 0.4f;

    // Component References (Cached automatically)
    private PlayerInputHandler inputHandler;
    private HealthSystem healthSystem;
    private FighterVisualManager visualManager;
    private Rigidbody rb;

    private Transform xrOrigin;
    private Vector3 fixedLocalPosition;
    private Vector3[] originalHandPositions;
    private float originalHandY;
    private bool isJumping = false;

    void Start()
    {
        currentState = FighterState.Idle;

        // Cache all modular component references
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();
        healthSystem = GetComponent<HealthSystem>();
        visualManager = GetComponent<FighterVisualManager>();

        xrOrigin = transform.parent;
        fixedLocalPosition = transform.localPosition;

        originalHandPositions = new Vector3[2];
        if (leftHand) originalHandPositions[0] = leftHand.localPosition;
        if (rightHand) originalHandPositions[1] = rightHand.localPosition;
        originalHandY = leftHand ? leftHand.localPosition.y : 0f;
    }

    void Update()
    {
        // State Machine Logic Loop
        switch (currentState)
        {
            case FighterState.Idle:
                ReturnToFixedPosition();
                ReturnHandsToRest();
                if (CheckArmExtension())
                {
                    currentState = FighterState.Attack;
                }
                break;

            case FighterState.Attack:
                ReturnToFixedPosition();
                ReturnHandsToRest();
                if (!CheckArmExtension())
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.Blocking:
                ReturnToFixedPosition();
                RaiseHandsForBlock();
                if (!inputHandler.IsBlocking)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.Dodge:
                LeanBack();
                if (!inputHandler.IsDodge)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.BeenHit:
                // Handled via external damage triggers
                break;
        }

        // Check Input-driven defensive states
        CheckInputStates();

        // Handle special weapon timer
        if (isSpecialActive)
        {
            specialTimer -= Time.deltaTime;
            if (specialTimer <= 0)
            {
                DeactivateSpecialPower();
            }
        }

        CheckFistBump();
        CheckGroundSlamJump();
    }

    void CheckInputStates()
    {
        if (currentState == FighterState.BeenHit) return;

        if (inputHandler.IsBlocking)
        {
            currentState = FighterState.Blocking;
        }
        else if (inputHandler.IsDodge)
        {
            currentState = FighterState.Dodge;
        }
    }

    bool CheckArmExtension()
    {
        if (!leftHand || !rightHand || !headCamera) return false;
        float leftZ = leftHand.position.z - headCamera.position.z;
        float rightZ = rightHand.position.z - headCamera.position.z;
        return (leftZ > attackExtensionThreshold || rightZ > attackExtensionThreshold);
    }

    void CheckFistBump()
    {
        if (!isSpecialActive && leftHand != null && rightHand != null)
        {
            float distance = Vector3.Distance(leftHand.position, rightHand.position);
            if (distance < fistBumpDistanceThreshold)
            {
                ActivateSpecialPower();
            }
        }
    }

    void ActivateSpecialPower()
    {
        isSpecialActive = true;
        specialTimer = specialDuration;
        Debug.Log("Special Power Activated! Fists are super-sized.");
        if (leftHand != null) leftHand.localScale = Vector3.one * 1.5f;
        if (rightHand != null) rightHand.localScale = Vector3.one * 1.5f;
    }

    void DeactivateSpecialPower()
    {
        isSpecialActive = false;
        Debug.Log("Special Power Ended.");
        if (leftHand != null) leftHand.localScale = Vector3.one;
        if (rightHand != null) rightHand.localScale = Vector3.one;
    }

    void CheckGroundSlamJump()
    {
        if (!isJumping && leftHand.position.y < 0.4f && rightHand.position.y < 0.4f)
        {
            isJumping = true;
            currentState = FighterState.Dodge;
            StartCoroutine(JumpSequence());
        }
    }

    IEnumerator JumpSequence()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.localPosition;
        Vector3 peakPos = fixedLocalPosition + Vector3.up * jumpHeight;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, peakPos, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        startPos = transform.localPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, fixedLocalPosition, elapsed / duration);
            yield return null;
        }

        transform.localPosition = fixedLocalPosition;
        isJumping = false;
    }

    void ReturnToFixedPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, fixedLocalPosition, 15f * Time.deltaTime);
    }

    void ReturnHandsToRest()
    {
        if (leftHand) leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, originalHandPositions[0], 10f * Time.deltaTime);
        if (rightHand) rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, originalHandPositions[1], 10f * Time.deltaTime);
    }

    void RaiseHandsForBlock()
    {
        float targetY = originalHandY + blockHandRaiseY;
        if (leftHand)
        {
            Vector3 pos = leftHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            leftHand.localPosition = pos;
        }
        if (rightHand)
        {
            Vector3 pos = rightHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            rightHand.localPosition = pos;
        }
    }

    void LeanBack()
    {
        Vector3 targetLocalPos = fixedLocalPosition + Vector3.back * dodgeBackDistance;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, 10f * Time.deltaTime);
    }

    // Called externally by the HitReceiver script when an enemy fist hits the player
    public void HandleHit(float damageAmount)
    {
        if (currentState == FighterState.Blocking)
        {
            Debug.Log("Attack blocked! Reduced damage taken.");
            healthSystem.TakeDamage(damageAmount * 0.2f);
        }
        else if (currentState == FighterState.Dodge)
        {
            Debug.Log("Dodged successfully! Zero damage.");
        }
        else
        {
            currentState = FighterState.BeenHit;
            healthSystem.TakeDamage(damageAmount);
            if (visualManager != null) visualManager.TriggerFlash(Color.red, 0.15f);
        }
    }
}