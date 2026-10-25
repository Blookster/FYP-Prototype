using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ============================================================================
//  P_FSM (the PLAYER's state machine)
// ============================================================================
// Everything not marked [NEW] / [CHANGED] / [FIX] is your original code.
//
//  [FIX] STATE DRIFT. Your CheckInputStates / ground slam wrote straight into the variable
//        "P_currentState = Blocking" instead of calling P_TransitionState(Blocking).
//        That changed the LABEL but never switched the state CLASS, so P_BlockState and
//        P_DodgeState never actually ran - and the label never went back to Idle either, so the
//        player stayed "Blocking" until their next punch. That would have made the player nearly
//        immune once the enemy can really hit. Rule: ALWAYS change state through
//        P_TransitionState, it keeps the label and the running class in sync.
//  [FIX] Start() grabs references BEFORE creating the states (the state constructors read
//        playerParameter).
//  [CHANGED] HandleHit really applies damage now (block = reduced, dodge/jump = none, otherwise
//        full damage + stagger).
//  [CHANGED] The special power scales the gloves relative to their ORIGINAL size
//        (before it forced scale to exactly 1.5 and back to exactly 1).

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthSystem))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(FighterVisualManager))]
[RequireComponent(typeof(PlayerParameter))]
// ^ Why it's coded like this: In Unity, scripts often crash with NullReferenceException if a required component is forgotten on the GameObject. These attributes tell Unity: "If you attach P_FSM to a capsule, automatically force-attach these other scripts too." It guarantees structural integrity before the game even presses Play.
public class P_FSM : MonoBehaviour
{
    public enum P_FighterState
    {
        Idle,
        Attack,
        Blocking,
        Dodge,
        BeenHit
    }

    public PlayerParameter playerParameter;

    [Header("State")]
    public P_FighterState P_currentState;

    private P_State P_currentStateClass;

    // Declarpation of PlayerState Dictionary
    private Dictionary<P_FighterState, P_State> P_States = new Dictionary<P_FighterState, P_State>();

    // [NEW] The gloves' size at game start, so the special can scale "relative to normal".
    private Vector3[] originalHandScales = new Vector3[2];

    void Start()
    {
        // [CHANGED] ---- 1. References FIRST (the states below read playerParameter) --------
        playerParameter = gameObject.GetComponent<PlayerParameter>();

        // Cache all modular component references
        playerParameter.rb = GetComponent<Rigidbody>();
        playerParameter.inputHandler = GetComponent<PlayerInputHandler>();
        playerParameter.healthSystem = GetComponent<HealthSystem>();
        playerParameter.visualManager = GetComponent<FighterVisualManager>();

        playerParameter.xrOrigin = transform.parent;
        playerParameter.fixedLocalPosition = transform.localPosition;

        playerParameter.originalHandPositions = new Vector3[2];
        if (playerParameter.leftHand) playerParameter.originalHandPositions[0] = playerParameter.leftHand.localPosition;
        if (playerParameter.rightHand) playerParameter.originalHandPositions[1] = playerParameter.rightHand.localPosition;
        playerParameter.originalHandY = playerParameter.leftHand ? playerParameter.leftHand.localPosition.y : 0f;

        // [NEW]
        if (playerParameter.leftHand) originalHandScales[0] = playerParameter.leftHand.localScale;
        if (playerParameter.rightHand) originalHandScales[1] = playerParameter.rightHand.localScale;

        // ---- 2. Initalization of PlayerState Dictionary -------------------------------
        #region
        P_States.Add(P_FighterState.Idle, new P_IdleState(this));
        P_States.Add(P_FighterState.Attack, new P_AttackState(this));
        P_States.Add(P_FighterState.Blocking, new P_BlockState(this));
        P_States.Add(P_FighterState.Dodge, new P_DodgeState(this));
        P_States.Add(P_FighterState.BeenHit, new P_BeenHitState(this));
        #endregion

        P_TransitionState(P_FighterState.Idle);
    }

    void Update()
    {
        P_currentStateClass.OnUpdate();

        CheckInputStates();

        // Handle special weapon timer
        if (playerParameter.isSpecialActive)
        {
            CountDownTheSpTimer();
        }

        CheckFistBump();
        CheckGroundSlamJump();
    }

    // [FIX] Uses P_TransitionState instead of assigning the label directly.
    public void CheckInputStates()
    {
        // While staggered the player has no control (this is also what stops block from
        // cancelling the stagger).
        if (P_currentState == P_FighterState.BeenHit) return;

        bool wantsBlock = playerParameter.inputHandler.IsBlocking;
        bool wantsDodge = playerParameter.inputHandler.IsDodge;

        // "&& P_currentState != X" = only switch if we aren't ALREADY in X. Otherwise we would
        // re-enter the state every frame (OnEnter would run 72 times a second).
        if (wantsBlock && P_currentState != P_FighterState.Blocking)
        {
            P_TransitionState(P_FighterState.Blocking);
        }
        else if (!wantsBlock && wantsDodge && P_currentState != P_FighterState.Dodge)
        {
            // Block has priority over dodge (same as your original if / else-if).
            P_TransitionState(P_FighterState.Dodge);
        }
        // Leaving Blocking / Dodge is done by P_BlockState / P_DodgeState themselves, when the button is released.
    }

    public bool CheckArmExtension()
    {
        if (!playerParameter.leftHand || !playerParameter.rightHand || !playerParameter.headCamera) return false;
        float leftZ = playerParameter.leftHand.position.z - playerParameter.headCamera.position.z;
        float rightZ = playerParameter.rightHand.position.z - playerParameter.headCamera.position.z;
        return (leftZ > playerParameter.attackExtensionThreshold || rightZ > playerParameter.attackExtensionThreshold);
    }

    void CheckFistBump()
    {
        if (!playerParameter.isSpecialActive && playerParameter.leftHand != null && playerParameter.rightHand != null)
        {
            float distance = Vector3.Distance(playerParameter.leftHand.position, playerParameter.rightHand.position);
            if (distance < playerParameter.fistBumpDistanceThreshold)
            {
                ActivateSpecialPower();
            }
        }
    }

    void ActivateSpecialPower()
    {
        playerParameter.isSpecialActive = true;
        playerParameter.specialTimer = playerParameter.specialDuration;
        Debug.Log("Special Power Activated! Fists are super-sized.");
        SetHandScale(playerParameter.specialHandScale);
    }

    void DeactivateSpecialPower()
    {
        playerParameter.isSpecialActive = false;
        Debug.Log("Special Power Ended.");
        SetHandScale(1.0f);
    }

    // [NEW] Scale each glove to (its original size x multiplier). 1 = normal.
    // Damage doesn't need touching here: PlayerParameter.GetCurrentDamage() reads isSpecialActive.
    void SetHandScale(float multiplier)
    {
        if (playerParameter.leftHand != null) playerParameter.leftHand.localScale = originalHandScales[0] * multiplier;
        if (playerParameter.rightHand != null) playerParameter.rightHand.localScale = originalHandScales[1] * multiplier;
    }

    void CheckGroundSlamJump()
    {
        if (playerParameter.leftHand == null || playerParameter.rightHand == null) return;

        if (!playerParameter.isJumping && playerParameter.leftHand.position.y < 0.4f && playerParameter.rightHand.position.y < 0.4f)
        {
            playerParameter.isJumping = true;

            // [CHANGED] The old line "P_currentState = Dodge" is gone (the state-drift bug above).
            // Being in the air counts as dodging through the isJumping flag; see HandleHit.
            StartCoroutine(JumpSequence());
        }
    }

    IEnumerator JumpSequence()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.localPosition;
        Vector3 peakPos = playerParameter.fixedLocalPosition + Vector3.up * playerParameter.jumpHeight;

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
            transform.localPosition = Vector3.Lerp(startPos, playerParameter.fixedLocalPosition, elapsed / duration);
            yield return null;
        }

        transform.localPosition = playerParameter.fixedLocalPosition;
        playerParameter.isJumping = false;
    }

    public void ReturnToFixedPosition()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, playerParameter.fixedLocalPosition, 15f * Time.deltaTime);
    }

    public void ReturnHandsToRest()
    {
        if (playerParameter.leftHand) playerParameter.leftHand.localPosition = Vector3.Lerp(playerParameter.leftHand.localPosition, playerParameter.originalHandPositions[0], 10f * Time.deltaTime);
        if (playerParameter.rightHand) playerParameter.rightHand.localPosition = Vector3.Lerp(playerParameter.rightHand.localPosition, playerParameter.originalHandPositions[1], 10f * Time.deltaTime);
    }

    public void RaiseHandsForBlock()
    {
        float targetY = playerParameter.originalHandY + playerParameter.blockHandRaiseY;
        if (playerParameter.leftHand)
        {
            Vector3 pos = playerParameter.leftHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            playerParameter.leftHand.localPosition = pos;
        }
        if (playerParameter.rightHand)
        {
            Vector3 pos = playerParameter.rightHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            playerParameter.rightHand.localPosition = pos;
        }
    }

    public void LeanBack()
    {
        Vector3 targetLocalPos = playerParameter.fixedLocalPosition + Vector3.back * playerParameter.dodgeBackDistance;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, 10f * Time.deltaTime);
    }

    // Called by the enemy's AttackState when a punch is in reach of the player's head.
    // [CHANGED] Now it really applies damage. It is a chain of "does something save the player?"
    // checks, from most protective to least, and the first one that applies wins.
    public void HandleHit(float damageAmount)
    {
        // 1) Already knocked out: nothing more to hit.
        if (playerParameter.healthSystem.IsDead) return;

        // 2) Staggered from the previous hit = short invincibility (see P_BeenHitState).
        if (P_currentState == P_FighterState.BeenHit) return;

        // 3) In the air from a ground slam = dodged. (isJumping is true for the whole jump.)
        if (playerParameter.isJumping)
        {
            Debug.Log("Dodged by jumping! Zero damage.");
            return;
        }

        // 4) Dodge button held: zero damage.
        if (P_currentState == P_FighterState.Dodge)
        {
            Debug.Log("Dodged successfully! Zero damage.");
            return;
        }

        // 5) Block held: take only a fraction (blockDamageMultiplier, 0.2 = 20%). No stagger.
        if (P_currentState == P_FighterState.Blocking)
        {
            float reduced = damageAmount * playerParameter.blockDamageMultiplier;
            playerParameter.healthSystem.TakeDamage(reduced);
            Debug.Log("Attack blocked! Took " + reduced + " instead of " + damageAmount + ".");
            return;
        }

        // 6) Nothing saved us: full damage.
        playerParameter.healthSystem.TakeDamage(damageAmount);
        if (playerParameter.visualManager != null) playerParameter.visualManager.TriggerFlash(Color.red, 0.15f);

        // Stagger (and i-frames) - but not if that hit just killed us (GameManager is handling the ending).
        if (!playerParameter.healthSystem.IsDead)
        {
            P_TransitionState(P_FighterState.BeenHit);
        }
    }

    public void P_TransitionState(P_FighterState type)
    {
        if (P_currentStateClass != null)
        {
            P_currentStateClass.OnExit();
        }
        P_currentState = type;
        P_currentStateClass = P_States[type];
        P_currentStateClass.OnEnter();
    }

    public void CountDownTheSpTimer()
    {
        playerParameter.specialTimer -= Time.deltaTime;
        if (playerParameter.specialTimer <= 0)
        {
            DeactivateSpecialPower();
        }
    }
}
