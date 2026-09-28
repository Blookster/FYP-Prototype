using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;
using System;
using System.Collections;

public class EnemyParameter : MonoBehaviour
{
    public static EnemyParameter instance;

    // Enemy Parameters
    [Header("Punch-Out Positioning")]
    public float attackExtension = 0.6f;
    public float dodgeBackDistance = 0.8f;
    // public float jumpHeight = 1.5f;

    [Header("Attack Timing")]
    public float attackWindupTime = 0.5f;
    public float attackActiveTime = 0.25f;
    public float attackRecoveryTime = 0.4f;
    public float counterAttackDelay = 0.5f;

    [Header("AI Pattern Settings")]
    public float decisionInterval = 3f;
    public float attackCooldown = 2f;
    public float blockingCooldown = 3f;
    public float blockDuration = 1f;
    public float dodgeDuration = 0.6f;
    public float idleWindowDuration = 3f;
    public float beenHitBodyDuration = 0.5f;
    public float beenHitHeadDuration = 0.5f;
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
    public HealthSystem healthSystem;
    public FighterVisualManager visualManager;
    public Rigidbody rb;
    public P_FSM p_fsm;

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
    public Vector3 targetPosition;
    public bool hasTargetPosition = false;

    [Header("Animator & Animations")]
    public Animator animator;
    
    void Awake()
    {
        if (instance == null) instance = this;
        else if(instance != null) Destroy(this);
    }

    void Start()
    {
        // Initalization of Components
        rb = gameObject.GetComponent<Rigidbody>();
        healthSystem = gameObject.GetComponent<HealthSystem>();
        visualManager = gameObject.GetComponent<FighterVisualManager>();
    }
}
