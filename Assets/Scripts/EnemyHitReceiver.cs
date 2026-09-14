using UnityEngine;

public class EnemyHitReceiver : MonoBehaviour
{
    private EnemyController enemyController;

    void Start()
    {
        enemyController = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            if (enemyController != null)
            {
                Vector3 hitDir = (enemyController.transform.position - other.transform.position).normalized;
                enemyController.TakeDamage(10f, hitDir);
            }
        }
    }
}