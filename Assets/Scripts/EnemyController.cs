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

    [Header("AI Pattern Settings")]
    public float decisionInterval = 2f;
    public float attackCooldown = 2f;
    public float blockDuration = 1f;
    public float dodgeDuration = 0.6f;
    public float idleWindowDuration = 3f;
    public float attackRange = 1.5f;

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
    private int attackPhase = 0; // 0=windup, 1=active, 2=recovery
    private Coroutine currentCoroutine;
    private bool inCounterAttackPattern = false;

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

        if (playerTarget == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) playerTarget = player.transform;
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(enemyHP);
        }
    }

    void Update()
    {
        if (playerTarget == null) return;

        decisionTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
        stateTimer -= Time.deltaTime;

        HandleStateTransitions();
        ExecuteCurrentState();
    }

    void HandleStateTransitions()
    {
        var player = playerTarget.GetComponent<PlayerController>();
        float dist = Vector3.Distance(transform.position, playerTarget.position);

        // BeenHit recovery
        if (currentEnemyState == FighterState.BeenHit && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Idle;
            inCounterAttackPattern = false;
        }

        // Attack sequence handled via coroutine
        if (currentEnemyState == FighterState.Attack && !isAttacking)
        {
            currentEnemyState = FighterState.IdleWindow;
            stateTimer = idleWindowDuration;
            inCounterAttackPattern = false;
        }

        // Blocking/Dodge/IdleWindow timers
        if ((currentEnemyState == FighterState.Blocking || currentEnemyState == FighterState.Dodge) && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Idle;
        }

        if (currentEnemyState == FighterState.IdleWindow && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Idle;
        }

        if (currentEnemyState == FighterState.Jump && stateTimer <= 0)
        {
            currentEnemyState = FighterState.Idle;
        }

        // AI Decision Making
        if (decisionTimer <= 0 && currentEnemyState == FighterState.Idle)
        {
            MakeDecision(dist, player);
            decisionTimer = decisionInterval;
        }
    }

    void MakeDecision(float dist, PlayerController player)
    {
        if (player == null) return;

        // PATTERN: Player attacks -> Block -> Counter Attack -> Idle Window
        if (player.currentState == PlayerController.FighterState.Attack)
        {
            currentEnemyState = FighterState.Blocking;
            stateTimer = blockDuration;
            inCounterAttackPattern = true;
            return;
        }

        // If we blocked and player finished attacking, FORCE counter attack
        if (inCounterAttackPattern && player.currentState != PlayerController.FighterState.Attack && attackTimer <= 0)
        {
            currentEnemyState = FighterState.Attack;
            StartCoroutine(AttackSequence());
            attackTimer = attackCooldown;
            return;
        }

        // Normal decision making when not in counter pattern
        if (!inCounterAttackPattern)
        {
            if (dist <= attackRange && attackTimer <= 0)
            {
                currentEnemyState = FighterState.Attack;
                StartCoroutine(AttackSequence());
                attackTimer = attackCooldown;
                return;
            }

            // Random behavior
            float rand = Random.value;
            if (rand < 0.4f)
            {
                currentEnemyState = FighterState.Idle;
            }
            else if (rand < 0.7f)
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
    }

    void ExecuteCurrentState()
    {
        switch (currentEnemyState)
        {
            case FighterState.Idle:
            case FighterState.IdleWindow:
                ReturnToFixedPosition();
                ReturnHandsToRest();
                break;

            case FighterState.Blocking:
                ReturnToFixedPosition();
                RaiseHandsForBlock();
                break;

            case FighterState.Dodge:
                LeanBack();
                break;

            case FighterState.Jump:
                // Handled in coroutine
                break;

            case FighterState.Attack:
                // Handled in coroutine
                break;

            case FighterState.BeenHit:
                // Handled in TakeDamage
                break;
        }
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;
        attackPhase = 0;

        // Phase 0: Windup (Tell)
        attackPhase = 0;
        SetAttackTellVisual(true);
        yield return new WaitForSeconds(attackWindupTime);

        // Phase 1: Active Attack - Dynamic reach to player
        attackPhase = 1;
        SetAttackTellVisual(false);
        
        // Calculate dynamic extension to reach player
        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        float requiredExtension = Mathf.Clamp(distToPlayer - 0.5f, 0.4f, 1.8f);
        
        // Scale hands up for punch impact feel
        Vector3 punchScale = Vector3.one * punchScaleMultiplier;
        if (leftHand) leftHand.localScale = punchScale;
        if (rightHand) rightHand.localScale = punchScale;
        
        ExtendHands(requiredExtension);
        EnableAttackColliders();
        
        // Play punch trail particles
        if (leftPunchTrail) leftPunchTrail.Play();
        if (rightPunchTrail) rightPunchTrail.Play();
        
        yield return new WaitForSeconds(attackActiveTime);

        // Phase 2: Recovery
        attackPhase = 2;
        DisableAttackColliders();
        
        // Stop particles
        if (leftPunchTrail) leftPunchTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        if (rightPunchTrail) rightPunchTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        
        RetractHands();
        
        // Reset hand scale
        if (leftHand) leftHand.localScale = Vector3.one;
        if (rightHand) rightHand.localScale = Vector3.one;
        
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

    void ReturnToFixedPosition()
    {
        transform.position = Vector3.Lerp(transform.position, fixedPosition, 15f * Time.deltaTime);
    }

    void LeanBack()
    {
        Vector3 targetPos = fixedPosition + Vector3.back * dodgeBackDistance;
        transform.position = Vector3.Lerp(transform.position, targetPos, 10f * Time.deltaTime);
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
            transform.position = Vector3.Lerp(startPos, peakPos, t);
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
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = fixedPosition;
    }

    void EnableAttackColliders()
    {
        if (leftHandCollider) leftHandCollider.enabled = true;
        if (rightHandCollider) rightHandCollider.enabled = true;
    }

    void DisableAttackColliders()
    {
        if (leftHandCollider) leftHandCollider.enabled = false;
        if (rightHandCollider) rightHandCollider.enabled = false;
    }

    public void TakeDamage(float damage, Vector3 hitDirection)
    {
        if (currentEnemyState == FighterState.Blocking)
        {
            enemyHP -= damage * 0.2f;
            Debug.Log("Enemy blocked! HP: " + enemyHP);
            StartCoroutine(HitFlash(Color.blue));
        }
        else if (currentEnemyState == FighterState.Dodge)
        {
            Debug.Log("Enemy dodged!");
            StartCoroutine(HitFlash(Color.green));
            return;
        }
        else
        {
            enemyHP -= damage;
            currentEnemyState = FighterState.BeenHit;
            stateTimer = 0.8f;
            inCounterAttackPattern = false;
            Debug.Log("Enemy hit! HP: " + enemyHP);
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
        // Trigger win UI here later
    }

    public void OnAttackHit(PlayerController player)
    {
        if (player != null && isAttacking && attackPhase == 1)
        {
            Vector3 hitDir = (player.transform.position - transform.position).normalized;
            player.TakeDamage(15f, hitDir);
        }
    }
}