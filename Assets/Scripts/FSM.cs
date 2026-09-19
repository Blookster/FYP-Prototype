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
    // [Header("Punch-Out Positioning")]
    // public float attackExtension = 0.6f;
    // public float dodgeBackDistance = 0.8f;
    // public float jumpHeight = 1.5f;

    // [Header("Attack Timing")]
    // public float attackWindupTime = 0.5f;
    // public float attackActiveTime = 0.25f;
    // public float attackRecoveryTime = 0.4f;
    public float counterAttackDelay = 0.5f;

    [Header("AI Pattern Settings")]
    // public float decisionInterval = 3f;
    public float attackCooldown = 2f;
    public float blockDuration = 1f;
    public float dodgeDuration = 0.6f;
    // public float idleWindowDuration = 3f;
    public float attackRange = 2.5f;

    // [Header("Blocking Visual")]
    // public float blockHandRaiseY = 0.35f;

    [Header("References")]
    public Transform playerTarget;
    // public Transform leftHand;
    // public Transform rightHand;
    // public Collider leftHandCollider;
    // public Collider rightHandCollider;

    // // Modular Component References
    // private HealthSystem healthSystem;
    // private FighterVisualManager visualManager;
    // private Rigidbody rb;
    public PlayerController playerController;

    // private Vector3 fixedPosition;
    // private Vector3[] originalHandPositions;
    // private float originalHandY;
    // private float decisionTimer;
    public float attackTimer;
    public float stateTimer;
    public bool isAttacking = false;
    // private int attackPhase = 0;
    public bool inCounterAttackPattern = false;
    // private bool useLeftPunch = true;
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

       TransitionState(FighterState.Idle); 
    }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, parameter.playerTarget.position);
        currentState.OnUpdate(dist);
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

    void MakeDecision(float dist)
    {

        // One of the attackstate function
        if (parameter.playerController.currentState == PlayerController.FighterState.Attack)
        {
            if (UnityEngine.Random.value < 0.2f)
            {
                TransitionState(FighterState.Dodge);
                // Old version
                #region 
                //currentEnemyState = FighterState.Dodge;
                #endregion
                
                //stateTimer = dodgeDuration;
            }
            else
            {
                TransitionState(FighterState.Blocking);
                // Old version
                #region 
                //currentEnemyState = FighterState.Blocking;
                #endregion

                //stateTimer = blockDuration;
                //inCounterAttackPattern = true;
            }
            return;
        }

        // All the state ref this code but Attackstate
        // Old version
        #region 
        // if (parameter.inCounterAttackPattern
        //     && parameter.playerController.currentState != PlayerController.FighterState.Attack
        //     && parameter.attackTimer <= 0)
        // {
        //     StartCoroutine(DelayedCounterAttack());
        //     return;
        // }
        #endregion

        // one of the IdleState function
        // Old version
        #region 
        // if (currentEnemyState == FighterState.Idle && !parameter.inCounterAttackPattern)
        // {
        //     float rand = UnityEngine.Random.value;
        //     if (dist <= attackRange && attackTimer <= 0)
        //     {
        //         if (rand < 0.5f)
        //         {
        //             currentEnemyState = FighterState.Attack;
        //             StartCoroutine(AttackSequence());
        //             attackTimer = attackCooldown;
        //         }
        //         else if (rand < 0.8f)
        //         {
        //             currentEnemyState = FighterState.Blocking;
        //             stateTimer = blockDuration;
        //         }
        //         else
        //         {
        //             currentEnemyState = FighterState.Dodge;
        //             stateTimer = dodgeDuration;
        //         }
        //     }
        //     else
        //     {
        //         currentEnemyState = (rand < 0.6f) ? FighterState.Idle : (rand < 0.8f ? FighterState.Blocking : FighterState.Dodge);
        //         if (currentEnemyState == FighterState.Blocking) stateTimer = blockDuration;
        //         if (currentEnemyState == FighterState.Dodge) stateTimer = dodgeDuration;
        //     }
        // }
        #endregion
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

    // All the state ref this code but Attackstate
    public void StateCounterAttackMode()
    {
        if (parameter.inCounterAttackPattern
            && parameter.playerController.currentState != PlayerController.FighterState.Attack
            && parameter.attackTimer <= 0)
        {
            StartCoroutine(DelayedCounterAttack());
            return;
        }
    }

    

}
