using UnityEngine;

public class HitReceiver : MonoBehaviour
{
    private PlayerController playerController;
    private EnemyController enemyController;

    void Start()
    {
        // Automatically check which controller lives on this object or its parents
        playerController = GetComponentInParent<PlayerController>();
        enemyController = GetComponentInParent<EnemyController>();

        if (playerController == null && enemyController == null)
        {
            Debug.LogWarning($"[HitReceiver] No PlayerController or EnemyController found on parent of {gameObject.name}!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Scenario A: This HitReceiver belongs to the Player, and an Enemy Hand touched it
        if (playerController != null && other.CompareTag("EnemyHand"))
        {
            Debug.Log($"[HitReceiver] Player hit by {other.name}");
            playerController.HandleHit(15f); // Pass default punch damage
        }
        // Scenario B: This HitReceiver belongs to the Enemy, and a Player Hand touched it
        else if (enemyController != null && other.CompareTag("PlayerHand"))
        {
            Debug.Log($"[HitReceiver] Enemy hit by {other.name}");
            Vector3 hitDir = (enemyController.transform.position - other.transform.position).normalized;
            enemyController.HandleHit(10f, hitDir); // Pass damage and hit direction
        }
    }
}