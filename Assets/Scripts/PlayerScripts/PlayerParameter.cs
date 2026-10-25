using UnityEngine;

public class PlayerParameter : MonoBehaviour
{
    public static PlayerParameter instance;

    // Player Parameter
    #region
    [Header("Player ATK")]
    public float atk = 10.0f;

    [Header("Tracking References")]
    public Transform headCamera;
    public Transform leftHand;
    public Transform rightHand;

    [Header("Special Weapon Power Settings")]
    public bool isSpecialActive = false;
    public float specialDuration = 10f;
    public float specialDamage = 20f;
    public float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f;
    [Tooltip("[NEW] How much bigger the gloves get during the special (1.5 = 50% bigger).")]
    public float specialHandScale = 1.5f;

    [Header("Punch-Out Positioning & Movement")]
    public float dodgeBackDistance = 0.8f;
    public float jumpHeight = 1.5f;
    public float blockHandRaiseY = 0.35f;
    public float attackExtensionThreshold = 0.4f;

    // [NEW] ------------------------------------------------------------------------------
    [Header("Taking Damage")]
    [Tooltip("Blocking turns a hit into this fraction of the damage (0.2 = take 20%).")]
    public float blockDamageMultiplier = 0.2f;

    [Tooltip("Seconds the player is staggered after a hit. They can't be hit again during this time (so one punch can't chain into three).")]
    public float hitStunDuration = 0.5f;
    // -------------------------------------------------------------------------------------

    // Component References (Cached automatically)
    public PlayerInputHandler inputHandler;
    public HealthSystem healthSystem;
    public FighterVisualManager visualManager;
    public Rigidbody rb;

    public Transform xrOrigin;
    public Vector3 fixedLocalPosition;
    public Vector3[] originalHandPositions;
    public float originalHandY;
    public bool isJumping = false;

    [Header("Debug")]
    public float currentDamage;

    #endregion

    void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != null) Destroy(this);
    }

    // [NEW] The ONE place that decides how hard the player hits right now.
    // Direct punches (E_HitReceiver) and bubbles (AttackPointManager) both ask this, so the
    // fist-bump special raises damage for everything, and the "after the duration the damage
    // goes back down" rule can't be forgotten in one of them.
    public float GetCurrentDamage()
    {
        currentDamage = isSpecialActive ? specialDamage : atk; // shows in the Inspector for debugging
        return currentDamage;
    }
}
