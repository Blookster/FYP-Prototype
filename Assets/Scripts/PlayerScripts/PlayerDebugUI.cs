
using UnityEngine;
using TMPro;

public class PlayerDebugUI : MonoBehaviour
{
    public static PlayerDebugUI Instance { get; private set; }

    [Header("UI Text References")]
    [SerializeField] private TMP_Text hitResultText;
    [SerializeField] private TMP_Text damageTakenText;
    [SerializeField] private TMP_Text blockStatusText;
    [SerializeField] private TMP_Text currentHpText;

    [Header("Player Reference")]
    [SerializeField] private PlayerController playerController;

    private void Awake()
    {
        // Make this UI accessible to the combat scripts.
        Instance = this;
    }

    private void Start()
    {
        // Automatically find the player if no reference was assigned.
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }

        // Display the initial values.
        hitResultText?.SetText("Last Hit: Waiting...");
        damageTakenText?.SetText("Damage Taken: 0");
        blockStatusText?.SetText("Block Status: Not Tested");

        RefreshHealth();
    }

    // Called by PlayerController whenever an attack hits the player.
    public void ReportHit(float damageTaken, bool wasBlocked)
    {
        if (wasBlocked)
        {
            hitResultText?.SetText("Last Hit: BLOCKED");
            blockStatusText?.SetText("Block Status: YES");
        }
        else
        {
            hitResultText?.SetText("Last Hit: BODY HIT");
            blockStatusText?.SetText("Block Status: NO");
        }

        damageTakenText?.SetText(
            $"Damage Taken: {damageTaken:0.##}"
        );

        RefreshHealth();
    }

    // Update the displayed HP using the player's HealthSystem.
    public void RefreshHealth()
    {
        if (playerController == null)
            return;

        P_HealthSystem health = playerController.GetComponent<P_HealthSystem>();

        if (health == null)
            return;

        currentHpText?.SetText(
            $"Player HP: {health.currentHp:0.##} / {health.maxHp:0.##}"
        );
    }
}
