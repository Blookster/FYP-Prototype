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
    public float specialTimer;
    public float fistBumpDistanceThreshold = 0.15f;

    [Header("Punch-Out Positioning & Movement")]
    public float dodgeBackDistance = 0.8f;
    public float jumpHeight = 1.5f;
    public float blockHandRaiseY = 0.35f;
    public float attackExtensionThreshold = 0.4f;

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
    #endregion

    void Awake()
    {
        if (instance == null) instance = this;
        else if(instance != null) Destroy(this);
    }
}
