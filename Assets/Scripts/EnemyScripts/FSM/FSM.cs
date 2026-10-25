using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ============================================================================
//  FSM (the ENEMY's state machine)
// ============================================================================
// Everything not marked [NEW] / [CHANGED] / [FIX] is exactly your code.
//
//  [FIX]     Removed unused "using UnityEditorInternal;" etc. (editor-only: breaks the Quest build).
//  [FIX]     Start() now grabs component references BEFORE creating the states. Your state
//            constructors read fsm.parameter (and parameter.p_fsm) the moment they are created,
//            but the old Start() filled those in AFTERWARDS. It only worked if you had dragged
//            "parameter" into the Inspector by hand.
//  [NEW]     Listens to HealthSystem.OnDied -> goes to the Dead state. (The old HealthSystem had
//            to know about this FSM; now it's the FSM that listens.)
//  [CHANGED] HandleHit takes a POSITIVE damage number and uses HealthSystem.TakeDamage.
//  [FIX]     Dead is final: nothing (a late punch, a bubble) can pull a dead enemy back into
//            a BeenHit state.
//  [NEW]     The enemy does nothing outside the "Playing" phase (countdown / game over).

public enum FighterState
{
    Idle,
    Attack,
    Blocking,
    Dodge,
    Jump,
    BeenHit_Body,
    BeenHit_Head,
    IdleWindow,
    Dead,
}

public class FSM : MonoBehaviour
{
    // Declarpation of State Dictionary
    private Dictionary<FighterState, IState> states = new Dictionary<FighterState, IState>();

    // Declarpation of Timer Dictionary
    private Dictionary<FighterState, float> timers = new Dictionary<FighterState, float>();

    // Declarpation of currentState
    private IState currentStateClass;
    public FighterState currentEnemyState;

    // Declarpation of Parameter
    public EnemyParameter parameter;

    public E_CooldownManager e_CooldownManager;

    void Start()
    {
        // [CHANGED] ---- 1. Component references FIRST -----------------------------------
        // Rule of thumb: anything other code needs while it is being constructed must be
        // assigned before that code runs. The state classes below read "parameter" in their
        // constructors, so "parameter" has to exist first.
        parameter = gameObject.GetComponent<EnemyParameter>();
        parameter.rb = GetComponent<Rigidbody>();
        parameter.healthSystem = GetComponent<HealthSystem>();
        parameter.visualManager = GetComponent<FighterVisualManager>();
        e_CooldownManager = gameObject.GetComponent<E_CooldownManager>();

        // Initalization of Position
        parameter.fixedPosition = transform.position;
        parameter.originalHandPositions = new Vector3[2];
        if (parameter.leftHand) parameter.originalHandPositions[0] = parameter.leftHand.localPosition;
        if (parameter.rightHand) parameter.originalHandPositions[1] = parameter.rightHand.localPosition;
        parameter.originalHandY = parameter.leftHand ? parameter.leftHand.localPosition.y : 0f;

        // [CHANGED] Find the player's P_FSM. Your old code had two copies of the same lookup and
        // crashed if playerTarget was empty. GetComponentInParent also checks the object itself,
        // and PlayerParameter.instance is a fallback so nothing has to be dragged in.
        if (parameter.playerTarget != null)
        {
            parameter.p_fsm = parameter.playerTarget.GetComponentInParent<P_FSM>();
        }
        if (parameter.p_fsm == null && PlayerParameter.instance != null)
        {
            parameter.p_fsm = PlayerParameter.instance.GetComponent<P_FSM>();
        }
        if (parameter.playerTarget == null && parameter.p_fsm != null)
        {
            parameter.playerTarget = parameter.p_fsm.transform;
        }
        if (parameter.p_fsm == null)
        {
            Debug.LogError("[FSM] No player found. Assign 'Player Target' on EnemyParameter.");
        }

        // ---- 2. Now it is safe to create the states -----------------------------------
        // Initalization of State Dictionary
        #region
        states.Add(FighterState.Idle, new IdleState(this));
        states.Add(FighterState.Attack, new AttackState(this));
        states.Add(FighterState.Blocking, new BlockingState(this));
        states.Add(FighterState.Dodge, new DodgeState(this));
        states.Add(FighterState.Jump, new JumpState(this));
        states.Add(FighterState.BeenHit_Body, new BeenHit_Body_State(this));
        states.Add(FighterState.BeenHit_Head, new BeenHit_Head_State(this));
        states.Add(FighterState.IdleWindow, new IdleWindowState(this));
        states.Add(FighterState.Dead, new DeadState(this));
        #endregion

        // Initalization of timer Dictionary
        #region
        timers.Add(FighterState.Attack, parameter.attackCooldown);
        timers.Add(FighterState.Blocking, parameter.blockDuration);
        timers.Add(FighterState.Dodge, parameter.dodgeDuration);
        timers.Add(FighterState.IdleWindow, parameter.idleWindowDuration);
        timers.Add(FighterState.BeenHit_Body, parameter.beenHitBodyDuration);
        timers.Add(FighterState.BeenHit_Head, parameter.beenHitHeadDuration);
        #endregion

        // [NEW] ---- 3. Listen for our own death ---------------------------------------
        // "+=" subscribes HandleDeath to HealthSystem's OnDied event (see HealthSystem.cs).
        parameter.healthSystem.OnDied += HandleDeath;

        TransitionState(FighterState.Idle);

        // TransitionState for debug
        #region
        //TransitionState(FighterState.Attack);
        //TransitionState(FighterState.Blocking);
        #endregion
    }

    // [NEW] Always unsubscribe from events when this object is destroyed.
    void OnDestroy()
    {
        if (parameter != null && parameter.healthSystem != null)
        {
            parameter.healthSystem.OnDied -= HandleDeath;
        }
    }

    void Update()
    {
        // [NEW] Don't fight during the "get ready" countdown or after the game has ended.
        // (If there's no GameManager in the scene we just carry on as before.)
        if (GameManager.instance != null && GameManager.instance.State != GameManager.GameState.Playing) return;

        float dist = Vector3.Distance(transform.position, parameter.playerTarget.position);
        currentStateClass.OnUpdate(dist);
        ExecuteCurrentStateVisuals();

        // Timer count down
        parameter.stateTimer -= Time.deltaTime;

        // Check the StateTimer
        if (parameter.stateTimer <= 0)
        {
            ReturnToIdle();
        }
    }

    void FixedUpdate()
    {
        ExecuteCurrentStateMovement();
    }

    public void TransitionState(FighterState type)
    {
        // [FIX] Dead is a one-way street. Without this, a bubble landing on a corpse would call
        // HandleHit -> TransitionState(BeenHit) and the KO'd enemy would stand back up.
        if (currentEnemyState == FighterState.Dead) return;

        if (currentStateClass != null)
        {
            currentStateClass.OnExit();
        }

        // [NEW] If we are about to die mid-punch, cancel the punch coroutine so a dead enemy can't
        // still land a hit on the player a moment later.
        if (type == FighterState.Dead)
        {
            StopAllCoroutines();
            parameter.isAttacking = false;
        }

        currentEnemyState = type;
        currentStateClass = states[type];
        currentStateClass.OnEnter();
    }

    public void AutoSetTimer(FighterState type)
    {
        parameter.stateTimer = timers[type];
    }

    // Not done yet
    public IEnumerator DelayedCounterAttack()
    {
        parameter.inCounterAttackPattern = false;
        yield return new WaitForSeconds(parameter.counterAttackDelay);

        if (currentEnemyState == FighterState.Idle && parameter.attackTimer <= 0)
        {
            currentEnemyState = FighterState.Attack;
            //StartCoroutine(AttackSequence());
            parameter.attackTimer = parameter.attackCooldown;
        }
    }

    void ReturnToIdle()
    {
        switch (currentEnemyState)
        {
            case FighterState.Blocking:
            case FighterState.Dodge:
            case FighterState.Jump:
            case FighterState.IdleWindow:
            case FighterState.BeenHit_Body:
            case FighterState.BeenHit_Head:
                TransitionState(FighterState.Idle);
                break;

            case FighterState.Idle:
            case FighterState.Attack:
                break;
        }
    }

    public void ExecuteCurrentStateVisuals()
    {
        switch (currentEnemyState)
        {
            case FighterState.Idle:
            case FighterState.IdleWindow:
            case FighterState.Dodge:
                ReturnHandsToRest();
                break;
            case FighterState.Blocking:
                RaiseHandsForBlock();
                break;
        }
    }

    void ReturnHandsToRest()
    {
        if (parameter.leftHand)
        {
            parameter.leftHand.localPosition = Vector3.Lerp(parameter.leftHand.localPosition, parameter.originalHandPositions[0], 10f * Time.deltaTime);
        }
        if (parameter.rightHand)
        {
            parameter.rightHand.localPosition = Vector3.Lerp(parameter.rightHand.localPosition, parameter.originalHandPositions[1], 10f * Time.deltaTime);
        }
    }

    void RaiseHandsForBlock()
    {
        float targetY = parameter.originalHandY + parameter.blockHandRaiseY;
        if (parameter.leftHand)
        {
            Vector3 pos = parameter.leftHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            parameter.leftHand.localPosition = pos;
        }
        if (parameter.rightHand)
        {
            Vector3 pos = parameter.rightHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            parameter.rightHand.localPosition = pos;
        }
    }

    void ExecuteCurrentStateMovement()
    {
        switch (currentEnemyState)
        {
            case FighterState.Idle:
            case FighterState.IdleWindow:
            case FighterState.Blocking:
                SetTargetPosition(parameter.fixedPosition);
                break;
            case FighterState.Dodge:
                SetTargetPosition(parameter.fixedPosition + Vector3.back * parameter.dodgeBackDistance);
                break;
        }

        if (parameter.hasTargetPosition && parameter.rb != null)
        {
            parameter.rb.MovePosition(Vector3.MoveTowards(transform.position, parameter.targetPosition, 10f * Time.fixedDeltaTime));
            if (Vector3.Distance(transform.position, parameter.targetPosition) < 0.05f)
            {
                parameter.hasTargetPosition = false;
            }
        }
    }

    void SetTargetPosition(Vector3 pos)
    {
        parameter.targetPosition = pos;
        parameter.hasTargetPosition = true;
    }

    // [NEW] Called when HealthSystem says HP reached 0.
    void HandleDeath()
    {
        TransitionState(FighterState.Dead);
    }

    // [CHANGED] "damage" is now a POSITIVE number (10 = ten damage).
    // Old: HandleHit(-10f, ...) relied on the minus sign, so a new script calling HandleHit(20f)
    // would have HEALED the enemy. Now the sign can't be wrong.
    // Used by BOTH direct punches (E_HitReceiver) and bubbles (AttackPointManager).
    public void HandleHit(float damage, bool isHeadHit)
    {
        // Dead enemies can't be hit again.
        if (currentEnemyState == FighterState.Dead) return;

        StopAllCoroutines();
        parameter.isAttacking = false;

        // Hurt FIRST. If this hit kills him, HealthSystem fires OnDied -> HandleDeath ->
        // state becomes Dead right now.
        parameter.healthSystem.TakeDamage(damage);

        // ...so if he just died we must NOT then switch to a BeenHit state on top of it.
        if (currentEnemyState == FighterState.Dead) return;

        if (isHeadHit) TransitionState(FighterState.BeenHit_Head);
        else TransitionState(FighterState.BeenHit_Body);
    }
}
