using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionAsset inputActions;
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction leftGripAction;
    private InputAction rightGripAction;
    private InputAction leftStickClickAction;
    private InputAction rightStickClickAction;

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
    public Transform headCamera;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Special Weapon Power Settings")]
    public bool isSpecialActive = false;
    public float specialDuration = 10f;
    private float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f;

    [Header("Punch-Out Positioning")]
    public float dodgeBackDistance = 0.8f;
    public float jumpHeight = 1.5f;
    public float blockHandRaiseY = 0.35f;

    [Header("Combat Settings")]
    public float attackExtensionThreshold = 0.4f;
    private bool isBlockingInput = false;
    private bool isDodgeInput = false;

    [Header("UI")]
    public HealthBarUI healthBarUI;

    [Header("Hit Feedback")]
    public Material bodyMaterial;
    public Color hitFlashColor = Color.red;
    public float hitFlashDuration = 0.15f;
    public float hitPauseDuration = 0.05f;

    private Rigidbody rb;
    private Transform xrOrigin;
    private Vector3 fixedLocalPosition;
    private Vector3[] originalHandPositions;
    private float originalHandY;

    void Start()
    {
        currentState = FighterState.Idle;
        rb = GetComponent<Rigidbody>();
        xrOrigin = transform.parent;
        fixedLocalPosition = transform.localPosition;
        originalHandPositions = new Vector3[2];
        if (leftHand) originalHandPositions[0] = leftHand.localPosition;
        if (rightHand) originalHandPositions[1] = rightHand.localPosition;
        originalHandY = leftHand ? leftHand.localPosition.y : 0f;

        SetupInputActions();

        if (healthBarUI != null)
        {
            healthBarUI.Initialize(playerHP, headCamera);
        }
    }

    void SetupInputActions()
    {
        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player");
            if (playerMap != null)
            {
                moveAction = playerMap.FindAction("Move");
                attackAction = playerMap.FindAction("Attack");
                leftGripAction = playerMap.FindAction("GripLeft");
                rightGripAction = playerMap.FindAction("GripRight");
                leftStickClickAction = playerMap.FindAction("StickClickLeft");
                rightStickClickAction = playerMap.FindAction("StickClickRight");

                moveAction?.Enable();
                attackAction?.Enable();
                leftGripAction?.Enable();
                rightGripAction?.Enable();
                leftStickClickAction?.Enable();
                rightStickClickAction?.Enable();
            }
        }
    }

    void OnDestroy()
    {
        moveAction?.Disable();
        attackAction?.Disable();
        leftGripAction?.Disable();
        rightGripAction?.Disable();
        leftStickClickAction?.Disable();
        rightStickClickAction?.Disable();
    }

    void Update()
    {
        UpdatePlayerInputs();

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
                if (!isBlockingInput)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.Dodge:
                LeanBack();
                if (!isDodgeInput)
                {
                    currentState = FighterState.Idle;
                }
                break;

            case FighterState.BeenHit:
                break;
        }

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

    void UpdatePlayerInputs()
    {
        bool bothGripsHeld = leftGripAction != null && rightGripAction != null &&
                            leftGripAction.IsPressed() && rightGripAction.IsPressed();

        bool stickClickPressed = (leftStickClickAction != null && leftStickClickAction.WasPressedThisFrame()) ||
                                 (rightStickClickAction != null && rightStickClickAction.WasPressedThisFrame());

        isBlockingInput = bothGripsHeld;
        isDodgeInput = stickClickPressed;

        if (isBlockingInput && currentState != FighterState.BeenHit)
        {
            currentState = FighterState.Blocking;
        }
        else if (isDodgeInput && currentState != FighterState.BeenHit)
        {
            currentState = FighterState.Dodge;
        }
    }

    bool CheckArmExtension()
    {
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
        Debug.Log("Special Power Activated! Fists are super-sized/powered.");
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
        if (leftHand.position.y < 0.4f && rightHand.position.y < 0.4f)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, playerJumpHeight, rb.linearVelocity.z);
                currentState = FighterState.Dodge;
                Debug.Log("Ground Slam Jump Triggered!");
            }
        }
    }

    void ReturnToFixedPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, fixedLocalPosition, 15f * Time.deltaTime);
    }

    void ReturnHandsToRest()
    {
        if (leftHand)
        {
            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, originalHandPositions[0], 10f * Time.deltaTime);
        }
        if (rightHand)
        {
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, originalHandPositions[1], 10f * Time.deltaTime);
        }
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

    public void TakeDamage(float damageAmount, Vector3 hitDirection = default)
    {
        if (currentState == FighterState.Blocking)
        {
            Debug.Log("Attack blocked! Reduced damage taken.");
            playerHP -= (damageAmount * 0.2f);
        }
        else if (currentState == FighterState.Dodge)
        {
            Debug.Log("Dodged successfully! Zero damage.");
            return;
        }
        else
        {
            currentState = FighterState.BeenHit;
            playerHP -= damageAmount;
            Debug.Log("Hit! Player HP remaining: " + playerHP);

            // Visual feedback
            StartCoroutine(HitFlashRoutine());
            StartCoroutine(HitPauseRoutine());

            if (playerHP <= 0)
            {
                Debug.Log("Player Knocked Out (KO)! Enemy Wins!");
            }
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(playerHP);
        }
    }

    IEnumerator HitFlashRoutine()
    {
        if (bodyMaterial != null)
        {
            bodyMaterial.EnableKeyword("_EMISSION");
            bodyMaterial.SetColor("_EmissionColor", hitFlashColor * 3f);
            yield return new WaitForSeconds(hitFlashDuration);
            bodyMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    IEnumerator HitPauseRoutine()
    {
        Time.timeScale = 0.02f;
        yield return new WaitForSecondsRealtime(hitPauseDuration);
        Time.timeScale = 1f;
    }
}