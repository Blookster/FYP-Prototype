using UnityEngine;

public class PlayerHitReceiver : MonoBehaviour
{
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyHand"))
        {
            Debug.Log($"[PlayerHitReceiver] Hit by {other.name} on {gameObject.name}");
            if (playerController != null)
            {
                Vector3 hitDir = (playerController.transform.position - other.transform.position).normalized;
                playerController.TakeDamage(15f, hitDir);
            }
        }
    }
}