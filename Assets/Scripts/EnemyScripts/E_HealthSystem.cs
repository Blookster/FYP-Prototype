using System;
using UnityEngine;
using UnityEngine.UI;

public class E_HealthSystem : MonoBehaviour
{
    public static E_HealthSystem instance;

    [Header("References")]
    public EnemyParameter enemyParameter;

    [Header("UI Sliders")]
    public Slider hpSlider;
    public Slider guardSlider;

    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnGuardChanged;
    public event Action OnGuardBroken;
    public event Action OnDied;

    public bool IsDead => enemyParameter != null ? enemyParameter.IsDead : true;
    public bool guardBreak => enemyParameter != null ? enemyParameter.guardBreak : true;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        if (enemyParameter == null)
        {
            enemyParameter = GetComponent<EnemyParameter>();
        }

        else if (enemyParameter != null)
        {
            enemyParameter.currentHp = enemyParameter.maxHp;
            enemyParameter.currentGuard = enemyParameter.maxGuard;
        }
    }

    void Start()
    {
        RefreshUI();
    }

    public void TakeDamage(float amount)
    {
        float damage = Mathf.Abs(amount);
        if (enemyParameter == null || IsDead) return;

        // 1. Drain Guard first while the enemy still has shield remaining
        if (enemyParameter.currentGuard > 0f)
        {
            float guardDamage = Mathf.Min(enemyParameter.currentGuard, damage);
            ChangeSheild(-guardDamage);
            damage -= guardDamage;

            Debug.Log($"{gameObject.name} Guard Hit! Guard: {enemyParameter.currentGuard} / {enemyParameter.maxGuard}");

            if (enemyParameter.currentGuard <= 0f)
            {
                Debug.Log($"{gameObject.name} Guard Broken!");
                OnGuardBroken?.Invoke();
            }
        }

        // 2. Only reduce the red HP bar after Guard reaches 0
        if (damage > 0f && enemyParameter.currentGuard <= 0f)
        {
            ChangeHealth(-damage);
        }
    }

    public void ChangeHealth(float delta)
    {
        if (enemyParameter == null || IsDead) return;

        enemyParameter.currentHp = Mathf.Clamp(enemyParameter.currentHp + delta, 0.0f, enemyParameter.maxHp);

        RefreshUI();
        Debug.Log($"{gameObject.name} HP: {enemyParameter.currentHp} / {enemyParameter.maxHp}");

        OnHealthChanged?.Invoke(enemyParameter.currentHp, enemyParameter.maxHp);

        if (IsDead) OnDied?.Invoke();
    }

    public void ChangeSheild(float delta)
    {
        if (enemyParameter == null) return;

        enemyParameter.currentGuard = Mathf.Clamp(enemyParameter.currentGuard + delta, 0.0f, enemyParameter.maxGuard);

        RefreshUI();
        OnGuardChanged?.Invoke(enemyParameter.currentGuard, enemyParameter.maxGuard);
    }

    public void ResetHealth()
    {
        if (enemyParameter == null) return;
        enemyParameter.currentHp = enemyParameter.maxHp;
        enemyParameter.currentGuard = enemyParameter.maxGuard;
        RefreshUI();
        OnHealthChanged?.Invoke(enemyParameter.currentHp, enemyParameter.maxHp);
        OnGuardChanged?.Invoke(enemyParameter.currentGuard, enemyParameter.maxGuard);
    }

    void RefreshUI()
    {
        if (enemyParameter == null) return;

        // Update direct sliders on E_HealthSystem if assigned
        if (hpSlider != null)
        {
            hpSlider.maxValue = enemyParameter.maxHp;
            hpSlider.value = enemyParameter.currentHp;
        }

        if (guardSlider != null)
        {
            guardSlider.maxValue = enemyParameter.maxGuard;
            guardSlider.value = enemyParameter.currentGuard;
        }

        // Also keep UIManager sliders in sync
        if (UIManager.instance != null)
        {
            if (UIManager.instance.e_HpSlider != null)
            {
                UIManager.instance.e_HpSlider.maxValue = enemyParameter.maxHp;
                UIManager.instance.UpdateEnemyHpSlider(enemyParameter.currentHp);
            }

            if (UIManager.instance.e_GuardSlider != null)
            {
                UIManager.instance.e_GuardSlider.maxValue = enemyParameter.maxGuard;
                UIManager.instance.UpdateEnemyGuardSlider(enemyParameter.currentGuard);
            }
        }
    }
}