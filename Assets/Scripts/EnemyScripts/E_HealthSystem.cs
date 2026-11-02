using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class E_HealthSystem : MonoBehaviour
{
    public static E_HealthSystem instance;

    [Header("References")]
    public EnemyParameter enemyParameter;

    [Header("Health Bar Slider")]
    public Slider hpSlider;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    public bool IsDead => enemyParameter != null ? enemyParameter.IsDead : true;
    public bool guardBreak => enemyParameter != null ? enemyParameter.guardBreak : true;


    private E_HealthSystem()
    {
        
    }

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        if (enemyParameter == null)
        {
            enemyParameter = GetComponent<EnemyParameter>();
        }

        if (enemyParameter != null)
        {
            enemyParameter.currentHp = enemyParameter.maxHp;
            enemyParameter.currentGuard = enemyParameter.maxGuard;
        }
    }
    //GameManager.instance.SetEnemyHealthSystem(this);


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
        if (enemyParameter == null) return;
        if (IsDead) return;

        // Modify HP on EnemyParameter
        enemyParameter.currentHp = Mathf.Clamp(enemyParameter.currentHp + delta, 0.0f, enemyParameter.maxHp);

        RefreshUI();
        Debug.Log(gameObject.name + " HP: " + enemyParameter.currentHp + " / " + enemyParameter.maxHp);

        OnHealthChanged?.Invoke(enemyParameter.currentHp, enemyParameter.maxHp);

        if (IsDead) OnDied?.Invoke();
    }

    public void ChangeSheild(float delta)
    {
        if (enemyParameter == null) return;
        enemyParameter.currentGuard = Mathf.Clamp(enemyParameter.currentGuard + delta, 0.0f, enemyParameter.maxGuard);
        //currentGuard - PlayerParameter.atk;
    }

    public void ResetHealth()
    {
        if (enemyParameter == null) return;
        enemyParameter.currentHp = enemyParameter.maxHp;
        RefreshUI();
        OnHealthChanged?.Invoke(enemyParameter.currentHp, enemyParameter.maxHp);
    }

    // Keeps the Slider in sync. Null-checked so the player can use this component
    // without having a slider at all.
    void RefreshUI()
    {
        if (hpSlider == null || enemyParameter == null) return;
        hpSlider.maxValue = enemyParameter.maxHp;
        hpSlider.value = enemyParameter.currentHp;
    }
}
