using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class E_HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] public float currentHp;
    public float maxHp = 100.0f;

    [Header("Health Bar Slider")]
    public Slider hpSlider;

    [Header("Sheild")]
    [SerializeField] private float currentGuard;
    public float maxGuard = 100.0f;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => currentHp <= 0.0f;
    public bool guardBreak => currentGuard <= 0.0f;


    private E_HealthSystem()
    {
        
    }

    void Awake()
    {   
        currentHp = maxHp;
        currentGuard = maxGuard;
        //GameManager.instance.SetEnemyHealthSystem(this);
    }

    void Start()
    {
        RefreshUI();
    }

    // Always pass a positive number. Mathf.Abs makes a wrong-signed call harmless.
    public void TakeDamage(float amount)
    {

        ChangeHealth(-Mathf.Abs(amount));
    }

    // delta < 0 = damage, delta > 0 = healing.
    public void ChangeHealth(float delta)
    {
        // Dead things stay dead: this also guarantees OnDied can only fire ONCE.
        if (IsDead) return;

        // Clamp keeps HP between 0 and max so it never goes negative or overheals.
        currentHp = Mathf.Clamp(currentHp + delta, 0.0f, maxHp);

        RefreshUI();
        Debug.Log(gameObject.name + " HP: " + currentHp + " / " + maxHp);

        // "?." = "only call if someone has subscribed" (avoids a null error when nobody listens).
        OnHealthChanged?.Invoke(currentHp, maxHp);

        if (IsDead) OnDied?.Invoke();
    }

    public void ChangeSheild(float delta)
    {
        //currentGuard - PlayerParameter.atk;
    }

    public void ResetHealth()
    {
        currentHp = maxHp;
        RefreshUI();
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }

    // Keeps the Slider in sync. Null-checked so the player can use this component
    // without having a slider at all.
    void RefreshUI()
    {
        if (hpSlider == null) return;
        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;
    }
}
