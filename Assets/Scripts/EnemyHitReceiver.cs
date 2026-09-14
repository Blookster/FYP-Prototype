using UnityEngine;

public class EnemyHitReceiver : MonoBehaviour
{
    private EnemyController enemyController;

    void Start()
    {
        // Cache the EnemyController component on startup
        enemyController = GetComponent<EnemyController>();
    }

    // Automatically runs when another collider enters this object's trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object hitting us is tagged as a player hand
        if (other.CompareTag("PlayerHand"))
        {
            if (enemyController != null)
            {
                // Deal damage and trigger the hit state
                float damage = 10f;
                enemyController.enemyHP -= damage;
                enemyController.currentEnemyState = EnemyController.FighterState.BeenHit;

                Debug.Log("Enemy hit! Remaining HP: " + enemyController.enemyHP);

                if (enemyController.enemyHP <= 0)
                {
                    Debug.Log("Enemy Knocked Out!");
                }
            }
        }
    }
}