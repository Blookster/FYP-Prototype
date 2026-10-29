using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(E_HealthSystem))]
[RequireComponent(typeof(FighterVisualManager))]
public class EnemyController : MonoBehaviour
{
    public enum FighterState
    {
        Idle,
        Attack,
        Blocking,
        Dodge,
        Jump,
        BeenHit,
        IdleWindow
    }

    [Header("AI State")]
    public FighterState currentEnemyState = FighterState.Idle;

    [Header("Punch-Out Positioning")]
    public float attackExtension = 0.6f;
    public float dodgeBackDistance = 0.8f;
    public float jumpHeight = 1.5f;

    [Header("Attack Timing")]
    public float attackWindupTime = 0.5f;
    public float attackActiveTime = 0.25f;
    public float attackRecoveryTime = 0.4f;
    public float counterAttackDelay = 0.5f;

    [Header("AI Pattern Settings")]
    public float decisionInterval = 3f;
    public float attackCooldown = 2f;
    public float blockDuration = 1f;
    public float dodgeDuration = 0.6f;
    public float idleWindowDuration = 3f;
    public float attackRange = 2.5f;

    [Header("Blocking Visual")]
    public float blockHandRaiseY = 0.35f;

    [Header("Health Settings")]


    [Header("References")]
    public Transform playerTarget;
    public Transform leftHand;
    public Transform rightHand;
    public Collider leftHandCollider;
    public Collider rightHandCollider;

    // Modular Component References
    private E_HealthSystem healthSystem;
    private FighterVisualManager visualManager;
    private Rigidbody rb;
    private PlayerController playerController;

    private Vector3 fixedPosition;
    private Vector3[] originalHandPositions;
    private float originalHandY;
    private float decisionTimer;
    private float attackTimer;
    private float stateTimer;
    private bool isAttacking = false;
    private int attackPhase = 0;
    private bool inCounterAttackPattern = false;
    private bool useLeftPunch = true;
    private Vector3 targetPosition;
    private bool hasTargetPosition = false;

    [Header("Animator & Animations")]
    public Animator animator;

    void Start()
    {
        fixedPosition = transform.position;
        originalHandPositions = new Vector3[2];
        if (leftHand) originalHandPositions[0] = leftHand.localPosition;
        if (rightHand) originalHandPositions[1] = rightHand.localPosition;
        originalHandY = leftHand ? leftHand.localPosition.y : 0f;

        decisionTimer = decisionInterval;
        attackTimer = attackCooldown;

        if (leftHandCollider) leftHandCollider.enabled = false;
        if (rightHandCollider) rightHandCollider.enabled = false;

        // Cache components
        rb = GetComponent<Rigidbody>();
        healthSystem = GetComponent<E_HealthSystem>();
        visualManager = GetComponent<FighterVisualManager>();

        if (playerTarget != null)
        {
            playerController = playerTarget.GetComponent<PlayerController>();
        }
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null) playerTarget = playerController.transform;
        }



        Debug.Log("[EnemyController] Initialized. PlayerController found: " + (playerController != null));

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }


    void Update()
    {
        if (healthSystem != null && healthSystem.IsDead)
            return;

        if (playerController.currentPlayerState == PlayerController.FighterState.Attack && currentEnemyState == FighterState.Idle)
        {
            if (Random.value < 0.4f) // 40% chance to dodge when you throw a punch
            {
                currentEnemyState = FighterState.Dodge;
                stateTimer = dodgeDuration;
                return;
            }
        }

        decisionTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        stateTimer -= Time.deltaTime;

        HandleStateTransitions();
        ExecuteCurrentStateVisuals();

        if (animator == null) return;

        // Pass states to Animator parameters (Ensure these parameters exist in your Animator Controller window)
        animator.SetBool("IsBlocking", currentEnemyState == FighterState.Blocking);
        animator.SetBool("IsDodge", currentEnemyState == FighterState.Dodge);
    }

    void FixedUpdate()
    {
        if (healthSystem != null && healthSystem.IsDead)
            return;
        ExecuteCurrentStateMovement();
    }

    void HandleStateTransitions()
    {
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // BeenHit recovery
        if (currentEnemyState == FighterState.BeenHit && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Attack;
            StartCoroutine(AttackSequence());
            attackTimer = attackCooldown;
            inCounterAttackPattern = false;
            return;
        }

        // Attack sequence completion
        if (currentEnemyState == FighterState.Attack && !isAttacking)
        {
            currentEnemyState = FighterState.IdleWindow;
            stateTimer = idleWindowDuration;
            inCounterAttackPattern = false;
            return;
        }

        // The function of all State but Idle, BeenHit and Attack
        // Timer expirations for defense/windows
        if ((currentEnemyState == FighterState.Blocking || currentEnemyState == FighterState.Dodge || currentEnemyState == FighterState.IdleWindow || currentEnemyState == FighterState.Jump) && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Idle;
        }

        // AI Decision Making
        if (decisionTimer <= 0 && currentEnemyState == FighterState.Idle)
        {
            MakeDecision(dist);
            decisionTimer = decisionInterval;
        }
    }

    void MakeDecision(float dist)
    {
        if (playerController.currentPlayerState == PlayerController.FighterState.Attack && currentEnemyState == FighterState.Idle)
        {
            if (Random.value < 0.5f) // 50% chance to step back and dodge when you punch
            {
                currentEnemyState = FighterState.Dodge;
                stateTimer = dodgeDuration;
                animator.SetBool("IsDodge", true);
                return;
            }
        }

        if (inCounterAttackPattern && playerController.currentPlayerState != PlayerController.FighterState.Attack && attackTimer <= 0)
        {
            StartCoroutine(DelayedCounterAttack());
            return;
        }

        if (currentEnemyState == FighterState.Idle && !inCounterAttackPattern)
        {
            float rand = Random.value;
            if (dist <= attackRange && attackTimer <= 0)
            {
                if (rand < 0.5f)
                {
                    currentEnemyState = FighterState.Attack;
                    StartCoroutine(AttackSequence());
                    attackTimer = attackCooldown;
                }
                else if (rand < 0.8f)
                {
                    currentEnemyState = FighterState.Blocking;
                    stateTimer = blockDuration;
                }
                else
                {
                    currentEnemyState = FighterState.Dodge;
                    stateTimer = dodgeDuration;
                }
            }
            else
            {
                currentEnemyState = (rand < 0.6f) ? FighterState.Idle : (rand < 0.8f ? FighterState.Blocking : FighterState.Dodge);
                if (currentEnemyState == FighterState.Blocking) stateTimer = blockDuration;
                if (currentEnemyState == FighterState.Dodge) stateTimer = dodgeDuration;
            }
        }
    }

    IEnumerator DelayedCounterAttack()
    {
        inCounterAttackPattern = false;
        yield return new WaitForSeconds(counterAttackDelay);

        if (currentEnemyState == FighterState.Idle && attackTimer <= 0)
        {
            currentEnemyState = FighterState.Attack;
            StartCoroutine(AttackSequence());
            attackTimer = attackCooldown;
        }
    }

    void ExecuteCurrentStateVisuals()
    {
        switch (currentEnemyState)
        {
            case FighterState.Idle:
            case FighterState.IdleWindow:
                ReturnHandsToRest();
                break;
            case FighterState.Blocking:
                RaiseHandsForBlock();
                break;
            case FighterState.Dodge:
                ReturnHandsToRest();
                break;
        }
    }

    void ExecuteCurrentStateMovement()
    {
        switch (currentEnemyState)
        {
            case FighterState.Idle:
            case FighterState.IdleWindow:
            case FighterState.Blocking:
                SetTargetPosition(fixedPosition);
                break;
            case FighterState.Dodge:
                SetTargetPosition(fixedPosition + Vector3.back * dodgeBackDistance);
                break;
        }

        if (hasTargetPosition && rb != null)
        {
            rb.MovePosition(Vector3.MoveTowards(transform.position, targetPosition, 10f * Time.fixedDeltaTime));
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                hasTargetPosition = false;
            }
        }
    }

    void SetTargetPosition(Vector3 pos)
    {
        targetPosition = pos;
        hasTargetPosition = true;
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        attackPhase = 0;

        // Phase 0: Windup Tell (Yellow Flash via Visual Manager)
        attackPhase = 0;
        if (visualManager != null) visualManager.TriggerFlash(Color.yellow, attackWindupTime);

        // Randomly pick one of the 5 punch animations!
        string[] attackTriggers = { "Jab", "RightCross", "LeftHook", "RightHook", "LeftUppercut" };
        string chosenAttack = attackTriggers[Random.Range(0, attackTriggers.Length)];

        if (animator != null)
        {
            animator.SetTrigger(chosenAttack);
            Debug.Log($"[EnemyController] Throwing punch: {chosenAttack}");
        }

        yield return new WaitForSeconds(attackWindupTime);

        // Phase 1: Active Attack
        attackPhase = 1;

        Transform punchHand = useLeftPunch ? leftHand : rightHand;
        Collider punchCollider = useLeftPunch ? leftHandCollider : rightHandCollider;
        useLeftPunch = !useLeftPunch;

        if (punchHand) punchHand.localScale = Vector3.one * 1.2f;

        if (punchHand && playerController && playerController.playerParameter.headCamera)
        {
            Vector3 cameraPos = playerController.playerParameter.headCamera.position;
            float distance = Vector3.Distance(punchHand.position, cameraPos);
            float localExtension = Mathf.Clamp(distance - 0.2f, 0.3f, 1.8f);
            ExtendHand(punchHand, localExtension);
        }
        else
        {
            ExtendHand(punchHand, attackExtension);
        }

        if (punchCollider) punchCollider.enabled = true;

        // Keep the punch hitbox active for the attack window.
        // HitReceiver handles the collision if the glove actually connects.
        yield return new WaitForSeconds(attackActiveTime);

        // Phase 2: Recovery
        attackPhase = 2;
        if (punchCollider) punchCollider.enabled = false;
        RetractHand(punchHand);
        if (punchHand) punchHand.localScale = Vector3.one;

        yield return new WaitForSeconds(attackRecoveryTime);
        isAttacking = false;
        attackPhase = 0;
    }

    void ExtendHand(Transform hand, float extension)
    {
        if (!hand) return;
        int index = (hand == leftHand) ? 0 : 1;
        hand.localPosition = originalHandPositions[index] + Vector3.forward * extension;
    }

    void RetractHand(Transform hand)
    {
        if (!hand) return;
        int index = (hand == leftHand) ? 0 : 1;
        hand.localPosition = originalHandPositions[index];
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

    // Called when the player successfully hits the enemy.
    public void HandleHit(float damage, Vector3 hitDir, bool isHeadHit)
    {
        // Safety checks: don't process hits if required components are missing.
        if (healthSystem == null || healthSystem.IsDead)
            return;

        // Preserve your existing dodge behaviour.
        if (animator != null &&
            animator.GetCurrentAnimatorStateInfo(0).IsName("Dodge"))
        {
            Debug.Log("[Combat] Hit missed! Enemy is dodging.");
            EnemyDebugUI.enemyStateText = "Dodging (Invulnerable)";
            EnemyDebugUI.lastHitResultText = "Missed (Dodge)";
            return;
        }

        // Interrupt the current attack or delayed action.
        StopAllCoroutines();
        isAttacking = false;

        // Apply damage through the one authoritative health component.
        healthSystem.TakeDamage(damage);

        // Update the enemy's combat state.
        if (healthSystem.IsDead)
        {
            currentEnemyState = FighterState.BeenHit;

            if (leftHandCollider != null)
                leftHandCollider.enabled = false;

            if (rightHandCollider != null)
                rightHandCollider.enabled = false;

            if (animator != null)
                animator.SetTrigger("KO");

            EnemyDebugUI.enemyStateText = "KO'd";
            EnemyDebugUI.currentHP = healthSystem.CurrentHp;
            return;
        }

        currentEnemyState = FighterState.BeenHit;
        stateTimer = 0.5f;

        // Play the appropriate hit reaction.
        if (animator != null)
        {
            if (isHeadHit)
            {
                string trigger = Random.value > 0.5f
                    ? "HeadHitRight"
                    : "HeadHitLeft";

                animator.SetTrigger(trigger);
                EnemyDebugUI.lastHitResultText =
                    $"Head Hit (-{damage} HP)";
            }
            else
            {
                animator.SetTrigger("StomachHit");
                EnemyDebugUI.lastHitResultText =
                    $"Stomach Hit (-{damage} HP)";
            }
        }

        EnemyDebugUI.enemyStateText = "Been Hit!";
        EnemyDebugUI.currentHP = healthSystem.CurrentHp;
    }
}
