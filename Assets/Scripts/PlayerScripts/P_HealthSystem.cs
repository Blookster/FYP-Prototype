using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class P_HealthSystem : MonoBehaviour
{
    [Header("References")]
    public PlayerParameter playerParameter;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => playerParameter != null ? playerParameter.IsDead : true;
    public bool guardBreak => playerParameter != null ? playerParameter.guardBreak : true;

    void Awake()
    {
        if (playerParameter == null)
        {
            playerParameter = GetComponent<PlayerParameter>();
        }

        // Initialize health and shield on PlayerParameter
        if (playerParameter != null)
        {
            playerParameter.currentHp = playerParameter.maxHp;
            playerParameter.currentGuard = playerParameter.maxGuard;
        }
        //GameManager.instance.SetPlayerHealthSystem(this);
    }

    void Start()
    {

        UIManager.instance.UpdatePlayerHpSlider(playerParameter.currentHp);

    }

    // Always pass a positive number. Mathf.Abs makes a wrong-signed call harmless.
    public void TakeDamage(float amount)
    {

        ChangeHealth(-Mathf.Abs(amount));
    }

    // delta < 0 = damage, delta > 0 = healing.
    public void ChangeHealth(float delta)
    {
        if (playerParameter == null) return;
        // Dead things stay dead: this also guarantees OnDied can only fire ONCE.
        if (IsDead) return;

        // Clamp keeps HP between 0 and max so it never goes negative or overheals.
        playerParameter.currentHp = Mathf.Clamp(playerParameter.currentHp + delta, 0.0f, playerParameter.maxHp);

        UIManager.instance.UpdatePlayerHpSlider(playerParameter.currentHp);

        Debug.Log(gameObject.name + " HP: " + playerParameter.currentHp + " / " + playerParameter.maxHp);

        // "?." = "only call if someone has subscribed" (avoids a null error when nobody listens).
        OnHealthChanged?.Invoke(playerParameter.currentHp, playerParameter.maxHp);

        if (IsDead) OnDied?.Invoke();
    }

    public void ResetHealth()
    {
        if (playerParameter == null) return;
        playerParameter.currentHp = playerParameter.maxHp;

        UIManager.instance.UpdatePlayerHpSlider(playerParameter.currentHp);

        OnHealthChanged?.Invoke(playerParameter.currentHp, playerParameter.maxHp);
    }
    
}
