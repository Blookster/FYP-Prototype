using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// ============================================================================
//  HealthSystem  -  ONE health component used by BOTH the player and the enemy.
// ============================================================================
// WHAT CHANGED AND WHY
//  [FIX]  The old version held a reference to the enemy's FSM and called
//         fsm.TransitionState(Dead) when HP hit 0. The player has no FSM (it has P_FSM),
//         so the player dying would have crashed with a NullReferenceException.
//  [NEW]  It now raises EVENTS instead ("OnHealthChanged", "OnDied"). A health component
//         shouldn't know who is listening - the enemy's FSM, the GameManager, a health
//         bar, a sound effect can all subscribe without HealthSystem knowing about them.
//         This idea is called the OBSERVER pattern (or "publish / subscribe").
//  [NEW]  TakeDamage(amount) takes a POSITIVE number. The old ChangeHealth(-10) meant
//         "10 damage" only because of the minus sign - easy to get backwards. ChangeHealth
//         still exists (negative = hurt, positive = heal) so nothing old breaks.
public class E_HealthSystem : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private float currentHp;

    // [FormerlySerializedAs("mixHp")]: the old field was misspelled "mixHp". If we simply
    // renamed it, Unity would lose the value you typed in the Inspector (it saves values
    // by field NAME). This attribute says "when loading, also look for the old name",
    // so we can fix the typo without losing any scene/prefab data.
    [FormerlySerializedAs("mixHp")]
    public float maxHp = 100.0f;

    [Header("Optional UI - drag a Slider here to get a health bar")]
    [FormerlySerializedAs("enemyHpSlider")]   // same trick: it's used by the player too now
    public Slider hpSlider;

    // "event Action<float, float>" = a list of functions that get called when we say so.
    // Anyone can write  healthSystem.OnHealthChanged += MyFunction;  to be told about it.
    // The two floats are (current HP, max HP).
    public event Action<float, float> OnHealthChanged;
    public event Action OnDied;

    // Read-only properties: other scripts can LOOK at HP but must go through
    // TakeDamage / ChangeHealth to change it. (Protects the number from accidental edits.)
    public float CurrentHp => currentHp;
    public bool IsDead => currentHp <= 0.0f;

    // Awake (not Start) so that currentHp is already correct when OTHER scripts run their
    // Start(). Awake of every object runs before any Start of any object.

    private E_HealthSystem()
    {
        
    }

    void Awake()
    {
        currentHp = maxHp;
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
