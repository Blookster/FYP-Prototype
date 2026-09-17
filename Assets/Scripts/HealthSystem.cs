using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Configuration")]
    public float maxHP = 100f;
    private float currentHP;

    [Header("UI Reference")]
    public HealthBarUI healthBarUI;

    public bool IsDead => currentHP <= 0;

    void Start()
    {
        currentHP = maxHP;

        // Initialize the floating world-space health bar if assigned
        if (healthBarUI != null)
        {
            healthBarUI.Initialize(maxHP, transform);
        }
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        // Clamp health between 0 and maxHP so it never goes into weird negative numbers
        currentHP = Mathf.Clamp(currentHP - amount, 0f, maxHP);
        Debug.Log($"[HealthSystem] {gameObject.name} took {amount} damage. Remaining HP: {currentHP}");

        // Update the visual health bar UI
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHP);
        }

        // Check for Knockout condition
        if (IsDead)
        {
            HandleDeath();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0f, maxHP);
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(currentHP);
        }
    }

    void HandleDeath()
    {
        Debug.Log($"[HealthSystem] {gameObject.name} has been Knocked Out (KO)!");
        // We can hook round-end or game-over logic here later!
    }
}