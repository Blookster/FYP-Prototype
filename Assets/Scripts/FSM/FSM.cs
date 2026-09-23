using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using System;
using System.Collections;

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

[Serializable]
// Enemy Parameters
public class Parameter
{
    #region 
    [Header("Punch-Out Positioning")]
    public float attackExtension = 0.6f;
    // public float dodgeBackDistance = 0.8f;
    // public float jumpHeight = 1.5f;

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

    // [Header("Blocking Visual")]
    public float blockHandRaiseY = 0.35f;

    [Header("References")]
    public Transform playerTarget;
    public Transform leftHand;
    public Transform rightHand;
    public Collider leftHandCollider;
    public Collider rightHandCollider;

    // // Modular Component References
    // private HealthSystem healthSystem;
    public FighterVisualManager visualManager;
    // private Rigidbody rb;
    public PlayerController playerController;

    public Vector3 fixedPosition;
    public Vector3[] originalHandPositions;
    public float originalHandY;
    public float decisionTimer;
    public float attackTimer;
    public float stateTimer;
    public bool isAttacking = false;
    public int attackPhase = 0;
    public bool inCounterAttackPattern = false;
    public bool useLeftPunch = true;
    // private Vector3 targetPosition;
    // private bool hasTargetPosition = false;

    [Header("Animator & Animations")]
    public Animator animator;
    #endregion

}

public class FSM : MonoBehaviour
{
    // Declarpation of State Dictionary
    private Dictionary<FighterState, IState> states = new Dictionary<FighterState, IState>();
    
    // Declarpation of currentState
    [SerializeField]
    private IState currentState;
    public FighterState currentEnemyState;
    
    // Declarpation of Parameter
    public Parameter parameter;

        void Start()
    {
        // Initalization Dictionary 
        #region 
        states.Add(FighterState.Idle, new IdleState(this));
        states.Add(FighterState.Attack, new AttackState(this));
        states.Add(FighterState.Blocking, new BlockingState(this));
        states.Add(FighterState.Dodge, new DodgeState(this));
        states.Add(FighterState.Jump, new JumpState(this));
        states.Add(FighterState.BeenHit, new BeenHitState(this));
        states.Add(FighterState.IdleWindow, new IdleWindowState(this));
        #endregion

        parameter.fixedPosition = transform.position;
        parameter.originalHandPositions = new Vector3[2];
        if (parameter.leftHand) parameter.originalHandPositions[0] = parameter.leftHand.localPosition;
        if (parameter.rightHand) parameter.originalHandPositions[1] = parameter.rightHand.localPosition;
        parameter.originalHandY = parameter.leftHand ? parameter.leftHand.localPosition.y : 0f;

        if (parameter.playerTarget != null)
        {
            parameter.playerController = parameter.playerTarget.GetComponent<PlayerController>();
        }
        if (parameter.playerController == null)
        {
            parameter.playerController = FindObjectOfType<PlayerController>();
            if (parameter.playerController != null)
            {
                parameter.playerTarget = parameter.playerController.transform;
            }
        }
    
       //TransitionState(FighterState.Idle); 

       // TransitionState for debug
       #region 
       //TransitionState(FighterState.Attack);
       TransitionState(FighterState.Blocking);
       #endregion
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, parameter.playerTarget.position);
        currentState.OnUpdate(dist);

        // Timer count down
        parameter.stateTimer -= Time.deltaTime;

        // Check the StateTimer
        if (parameter.stateTimer <= 0)
        {
            ReturnToIdle();
        }
    }

    public void TransitionState(FighterState type)
    {
        if (currentState != null)
        {
            currentState.OnExit();
        }
        currentEnemyState = type;
        currentState = states[type];
        currentState.OnEnter();
    }

    IEnumerator DelayedCounterAttack()
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
            TransitionState(FighterState.Idle);
            break;

            case FighterState.Idle:
            case FighterState.Attack:
            case FighterState.BeenHit:
            return;
        }
    }

}
