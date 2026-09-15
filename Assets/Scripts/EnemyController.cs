using UnityEngine;
using System.Collections;

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

    [Header("Enemy Stats")]
    public float enemyHP = 100f;
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

    [Header("Visual Feedback")]
    public Material handMaterial;
    public Color attackTellColor = Color.yellow;
    public Color normalHandColor = Color.white;
    public float punchScaleMultiplier = 1.2f;

    [Header("Punch Trail Particles")]
    public ParticleSystem leftPunchTrail;
    public ParticleSystem rightPunchTrail;

    [Header("References")]
    public Transform playerTarget;
    public Transform leftHand;
    public Transform rightHand;
    public Transform enemyHead;
    public Collider leftHandCollider;
    public Collider rightHandCollider;
    public HealthBarUI healthBarUI;

    private Vector3 fixedPosition;
    private Vector3[] originalHandPositions;
    private float originalHandY;
    private float decisionTimer;
    private float attackTimer;
    private float stateTimer;
    private bool isAttacking = false;
    private int attackPhase = 0;
    private bool inCounterAttackPattern = false;
    private bool isAlternatingPunch = true;
    private bool useLeftPunch = true;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool hasTargetPosition = false;
    private PlayerController playerController;

    void Start()
    {
        fixedPosition = transform.position;
        originalHandPositions = new Vector3[2];
        if (leftHand) originalHandPositions[0] = leftHand.localPosition;
        if (rightHand) originalHandPositions[1] = rightHand.localPosition;
        originalHandY = leftHand ? leftHand.localPosition.y : 0f;

        decisionTimer = decisionInterval;
        attackTimer = attackCooldown;

        DisableAttackColliders();

        rb = GetComponent<Rigidbody>();
        playerController = playerTarget ? playerTarget.GetComponent<PlayerController>() : null;

        if (playerController == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerController = player;
                playerTarget = player.transform;
            }
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(enemyHP);
        }

        Debug.Log("[EnemyController] Initialized. PlayerController found: " + (playerController != null));
    }

    void Update()
    {
        if (playerTarget == null || playerController == null) return;

        decisionTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        stateTimer -= Time.deltaTime;

        HandleStateTransitions();
        ExecuteCurrentStateVisuals();
    }

    void FixedUpdate()
    {
        ExecuteCurrentStateMovement();
    }

    void HandleStateTransitions()
    {
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // BeenHit recovery
        if (currentEnemyState == FighterState.BeenHit && stateTimer <= 0)
        {
            Debug.Log("[EnemyController] BeenHit recovery → Attack (aggressive)");
            currentEnemyState = FighterState.Attack;
            StartCoroutine(AttackSequence());
            attackTimer = attackCooldown;
            inCounterAttackPattern = false;
            return;
        }

        // Attack sequence handled via coroutine
        if (currentEnemyState == FighterState.Attack && !isAttacking)
        {
            Debug.Log("[EnemyController] Attack finished → IdleWindow");
            currentEnemyState = FighterState.IdleWindow;
            stateTimer = idleWindowDuration;
            inCounterAttackPattern = false;
            return;
        }

        // Blocking/Dodge/IdleWindow timers
        if ((currentEnemyState == FighterState.Blocking || currentEnemyState == FighterState.Dodge) && stateTimer <= 0)
        {
            Debug.Log($"[EnemyController] {currentEnemyState} timer expired → Idle");
            currentEnemyState = FighterState.Idle;
        }

        if (currentEnemyState == FighterState.IdleWindow && stateTimer <= 0)
        {
            Debug.Log("[EnemyController] IdleWindow expired → Idle");
            currentEnemyState = FighterState.Idle;
        }

        if (currentEnemyState == FighterState.Jump && stateTimer <= 0)
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
        // PATTERN: Player attacks → Block → Counter Attack (delayed) → Idle Window
        if (playerController.currentState == PlayerController.FighterState.Attack)
        {
            // Dodge instinctively (20%) or block (80%)
            if (Random.value < 0.2f)
            {
                Debug.Log("[EnemyController] Player attacking → Instinctive DODGE");
                currentEnemyState = FighterState.Dodge;
                stateTimer = dodgeDuration;
            }
            else
            {
                Debug.Log("[EnemyController] Player attacking → BLOCK");
                currentEnemyState = FighterState.Blocking;
                stateTimer = blockDuration;
                inCounterAttackPattern = true;
            }
            return;
        }

        // Counter-attack pattern: after blocking, wait for player to stop attacking, then counter
        if (inCounterAttackPattern && playerController.currentState != PlayerController.FighterState.Attack && attackTimer <= 0)
        {
            Debug.Log("[EnemyController] Counter-attack trigger → ATTACK (delayed)");
            StartCoroutine(DelayedCounterAttack());
            return;
        }

        // Post-hit aggressive attack
        if (currentEnemyState == FighterState.Idle && !inCounterAttackPattern)
        {
            // Random decision during idle
            float rand = Random.value;
            if (dist <= attackRange && attackTimer <= 0)
            {
                if (rand < 0.5f)
                {
                    Debug.Log("[EnemyController] In range → ATTACK");
                    currentEnemyState = FighterState.Attack;
                    StartCoroutine(AttackSequence());
                    attackTimer = attackCooldown;
                }
                else if (rand < 0.8f)
                {
                    Debug.Log("[EnemyController] Random → BLOCK (preemptive)");
                    currentEnemyState = FighterState.Blocking;
                    stateTimer = blockDuration;
                }
                else
                {
                    Debug.Log("[EnemyController] Random → DODGE");
                    currentEnemyState = FighterState.Dodge;
                    stateTimer = dodgeDuration;
                }
            }
            else if (rand < 0.6f)
            {
                Debug.Log("[EnemyController] Random → IDLE");
                currentEnemyState = FighterState.Idle;
            }
            else if (rand < 0.8f)
            {
                Debug.Log("[EnemyController] Random → BLOCK (preemptive)");
                currentEnemyState = FighterState.Blocking;
                stateTimer = blockDuration;
            }
            else
            {
                Debug.Log("[EnemyController] Random → DODGE");
                currentEnemyState = FighterState.Dodge;
                stateTimer = dodgeDuration;
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

            case FighterState.Jump:
            case FighterState.Attack:
            case FighterState.BeenHit:
                // Handled in coroutines
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

            case FighterState.Jump:
            case FighterState.Attack:
            case FighterState.BeenHit:
                // Handled in coroutines
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
        Debug.Log("[EnemyController] AttackSequence STARTED");

        // Phase 0: Windup (Tell)
        attackPhase = 0;
        SetAttackTellVisual(true);
        yield return new WaitForSeconds(attackWindupTime);

        // Phase 1: Active Attack - Alternate punches to player camera
        attackPhase = 1;
        SetAttackTellVisual(false);
        Debug.Log("[EnemyController] Attack ACTIVE - enabling colliders");

        // Alternate left/right
        Transform punchHand = useLeftPunch ? leftHand : rightHand;
        Collider punchCollider = useLeftPunch ? leftHandCollider : rightHandCollider;
        ParticleSystem punchTrail = useLeftPunch ? leftPunchTrail : rightPunchTrail;
        
        useLeftPunch = !useLeftPunch; // Alternate for next attack

        // Scale punch hand
        Vector3 punchScale = Vector3.one * punchScaleMultiplier;
        if (punchHand) punchHand.localScale = punchScale;

        // Extend punch hand toward player's camera
        if (punchHand && playerController && playerController.headCamera)
        {
            Vector3 cameraPos = playerController.headCamera.position;
            Vector3 handWorldPos = punchHand.position;
            Vector3 direction = (cameraPos - handWorldPos).normalized;
            float distance = Vector3.Distance(handWorldPos, cameraPos);
            
            // Convert world distance to local Z extension
            Vector3 localForward = punchHand.parent.InverseTransformDirection(direction);
            float localExtension = Mathf.Clamp(distance - 0.2f, 0.3f, 1.8f);
            
            ExtendHand(punchHand, localExtension);
            Debug.Log($"[EnemyController] Punch extended {localExtension} units toward camera");
        }
        else
        {
            // Fallback
            ExtendHand(punchHand, attackExtension);
        }

        EnableAttackCollider(punchCollider);
        
        if (punchTrail) punchTrail.Play();

        // Wait half attack time, then deal damage
        yield return new WaitForSeconds(attackActiveTime * 0.5f);
        
        OnAttackHit(playerController);
        
        yield return new WaitForSeconds(attackActiveTime * 0.5f);

        // Phase 2: Recovery
        attackPhase = 2;
        DisableAttackCollider(punchCollider);
        
        if (punchTrail) punchTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        RetractHand(punchHand);
        
        if (punchHand) punchHand.localScale = Vector3.one;
        
        yield return new WaitForSeconds(attackRecoveryTime);

        isAttacking = false;
        attackPhase = 0;
    }

    void SetAttackTellVisual(bool active)
    {
        if (handMaterial != null)
        {
            handMaterial.EnableKeyword("_EMISSION");
            handMaterial.SetColor("_EmissionColor", active ? attackTellColor * 2f : Color.black);
        }
    }

    void ExtendHand(Transform hand, float extension)
    {
        if (!hand) return;
        int index = (hand == leftHand) ? 0 : 1;
        if (index < originalHandPositions.Length)
        {
            hand.localPosition = originalHandPositions[index] + Vector3.forward * extension;
        }
    }

    void RetractHand(Transform hand)
    {
        if (!hand) return;
        int index = (hand == leftHand) ? 0 : 1;
        if (index < originalHandPositions.Length)
        {
            hand.localPosition = originalHandPositions[index];
        }
    }

    void ExtendHands(float extension)
    {
        if (leftHand) leftHand.localPosition = originalHandPositions[0] + Vector3.forward * extension;
        if (rightHand) rightHand.localPosition = originalHandPositions[1] + Vector3.forward * extension;
    }

    void RetractHands()
    {
        if (leftHand) leftHand.localPosition = originalHandPositions[0];
        if (rightHand) rightHand.localPosition = originalHandPositions[1];
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

    void EnableAttackCollider(Collider col)
    {
        if (col)
        {
            col.enabled = true;
            Debug.Log($"[EnemyController] EnableAttackCollider: {col.name} = true");
        }
    }

    void DisableAttackCollider(Collider col)
    {
        if (col)
        {
            col.enabled = false;
            Debug.Log($"[EnemyController] DisableAttackCollider: {col.name} = false");
        }
    }

    void EnableAttackColliders()
    {
        EnableAttackCollider(leftHandCollider);
        EnableAttackCollider(rightHandCollider);
    }

    void DisableAttackColliders()
    {
        DisableAttackCollider(leftHandCollider);
        DisableAttackCollider(rightHandCollider);
    }

    public void OnAttackHit(PlayerController player)
    {
        Debug.Log($"[EnemyController] OnAttackHit called - isAttacking: {isAttacking}, attackPhase: {attackPhase}");
        if (player != null && isAttacking && attackPhase == 1)
        {
            Vector3 hitDir = (player.transform.position - transform.position).normalized;
            player.TakeDamage(15f, hitDir);
            Debug.Log("[EnemyController] Dealt damage to player");
        }
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        Debug.Log($"[EnemyController] TakeDamage called - State: {currentEnemyState}, Damage: {damage}, HP before: {enemyHP}");
        if (currentEnemyState == FighterState.Blocking)
        {
            enemyHP -= damage * 0.2f;
            Debug.Log($"[EnemyController] Enemy blocked! HP: {enemyHP}");
            StartCoroutine(HitFlash(Color.blue));
        }
        else if (currentEnemyState == FighterState.Dodge)
        {
            Debug.Log("[EnemyController] Enemy dodged!");
            StartCoroutine(HitFlash(Color.green));
            return;
        }
        else
        {
            enemyHP -= damage;
            currentEnemyState = FighterState.BeenHit;
            stateTimer = 0.8f;
            inCounterAttackPattern = false;
            Debug.Log($"[EnemyController] Enemy hit! HP: {enemyHP}");
            StartCoroutine(HitFlash(Color.red));
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(enemyHP);
        }

        if (enemyHP <= 0)
        {
            Die();
        }
    }

    IEnumerator HitFlash(Color color)
    {
        if (handMaterial != null)
        {
            Color originalEmission = handMaterial.GetColor("_EmissionColor");
            handMaterial.EnableKeyword("_EMISSION");
            handMaterial.SetColor("_EmissionColor", color * 2f);
            yield return new WaitForSeconds(0.1f);
            handMaterial.SetColor("_EmissionColor", originalEmission);
        }
    }

    void Die()
    {
        Debug.Log("Enemy Defeated! Player Wins!");
        enabled = false;
        DisableAttackColliders();
        if (handMaterial != null)
        {
            handMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    public void DoJump()
    {
        if (currentEnemyState != FighterState.Jump && currentEnemyState != FighterState.BeenHit)
        {
            currentEnemyState = FighterState.Jump;
            stateTimer = 0.5f;
            StartCoroutine(JumpSequence());
        }
    }

    IEnumerator JumpSequence()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.position;
        Vector3 peakPos = fixedPosition + Vector3.up * jumpHeight;

        // Up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 newPos = Vector3.Lerp(startPos, peakPos, t);
            if (rb != null) rb.MovePosition(newPos);
            else transform.position = newPos;
            yield return null;
        }

        // Down
        elapsed = 0f;
        startPos = transform.position;
        Vector3 endPos = fixedPosition;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            if (rb != null) rb.MovePosition(newPos);
            else transform.position = newPos;
            yield return null;
        }

        if (rb != null) rb.MovePosition(fixedPosition);
        else transform.position = fixedPosition;
    }
}