using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    private PlayerController playerController;
    private EnemyController enemyController;

    [Header("Hitbox Classification (For Enemy Only)")]
    [Tooltip("Check this if this collider is attached to the enemy's head/jaw hitbox.")]
    public bool isHeadHitbox = false;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
        enemyController = GetComponentInParent<EnemyController>();

        if (playerController == null && enemyController == null)
        {
            Debug.LogWarning($"[HitReceiver] No PlayerController or EnemyController found on parent of {gameObject.name}!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Scenario A: Player hit by Enemy
        if (playerController != null && other.CompareTag("EnemyHand"))
        {
            Debug.Log($"[HitReceiver] Player hit by {other.name}");
            playerController.HandleHit(15f);
        }
        // Scenario B: Enemy hit by Player
        else if (enemyController != null && other.CompareTag("PlayerHand"))
        {
            Debug.Log($"[HitReceiver] Enemy hit by {other.name} (Head: {isHeadHitbox})");
            Vector3 hitDir = (enemyController.transform.position - other.transform.position).normalized;

            // Pass the damage, direction, and whether it was a headshot!
            enemyController.HandleHit(10f, hitDir, isHeadHitbox);
        }
    }
}