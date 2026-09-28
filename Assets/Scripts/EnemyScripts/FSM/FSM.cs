using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using System;
using System.Collections;
using System.Threading;

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
    [SerializeField]
    private IState currentStateClass;
    public FighterState currentEnemyState;
    
    // Declarpation of Parameter
    public EnemyParameter parameter;

    public E_CooldownManager e_CooldownManager;

        void Start()
    {
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

        // Initalization of Components
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

        if (parameter.playerTarget != null)
        {
            parameter.p_fsm = parameter.playerTarget.GetComponent<P_FSM>();
        }
        if (parameter.p_fsm == null)
        {
            parameter.p_fsm = parameter.playerTarget.GetComponent<P_FSM>();
            if (parameter.p_fsm != null)
            {
                parameter.playerTarget = parameter.p_fsm.transform;
            }
        }
    
       TransitionState(FighterState.Idle);

       // TransitionState for debug
       #region 
       //TransitionState(FighterState.Attack);
       //TransitionState(FighterState.Blocking);
       #endregion
    }

    void Update()
    {
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
        if (currentStateClass != null)
        {
            currentStateClass.OnExit();
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

    public void HandleHit(float damage, bool isHeadHit)
    {
        StopAllCoroutines();
        parameter.isAttacking = false;

        if (isHeadHit) TransitionState(FighterState.BeenHit_Head);
        
        else TransitionState(FighterState.BeenHit_Body);

        parameter.healthSystem.ChangeHealth(damage);
    }

}
