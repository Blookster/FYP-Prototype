using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text timerText;

    [Header("HP Sliders")]
    public Slider p_HpSlider; //player
    public Slider e_HpSlider; //enemy

    [Header("Guard Sliders")]
    public Slider p_GuardSlider;
    public Slider e_GuardSlider;


    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        // Initialize Enemy Slider Max & Starting Values
        if (EnemyParameter.instance != null)
        {
            if (e_HpSlider != null)
            {
                e_HpSlider.maxValue = EnemyParameter.instance.maxHp;
                e_HpSlider.value = EnemyParameter.instance.currentHp;
            }
            if (e_GuardSlider != null)
            {
                e_GuardSlider.maxValue = EnemyParameter.instance.maxGuard;
                e_GuardSlider.value = EnemyParameter.instance.currentGuard;
            }
        }

        // Initialize Player Slider Max & Starting Values
        if (PlayerParameter.instance != null)
        {
            if (p_HpSlider != null)
            {
                p_HpSlider.maxValue = PlayerParameter.instance.maxHp;
                p_HpSlider.value = PlayerParameter.instance.currentHp;
            }
            if (p_GuardSlider != null)
            {
                p_GuardSlider.maxValue = PlayerParameter.instance.maxGuard;
                p_GuardSlider.value = PlayerParameter.instance.currentGuard;
            }
        }
    }

    public void UpdateTimerUI(int time)
    {
        if (timerText != null) timerText.text = "Timer " + time;
    }

    public void UpdatePlayerHpSlider(float hp)
    {
        if (p_HpSlider != null)
        {
            if (PlayerParameter.instance != null) p_HpSlider.maxValue = PlayerParameter.instance.maxHp;
            p_HpSlider.value = hp;
        }
    }

    public void UpdateEnemyHpSlider(float hp)
    {
        if (e_HpSlider != null)
        {
            if (EnemyParameter.instance != null) e_HpSlider.maxValue = EnemyParameter.instance.maxHp;
            e_HpSlider.value = hp;
        }
    }

    public void UpdateEnemyGuardSlider(float guard)
    {
        if (e_GuardSlider != null)
        {
            if (EnemyParameter.instance != null) e_GuardSlider.maxValue = EnemyParameter.instance.maxGuard;
            e_GuardSlider.value = guard;
        }
    }

    public void UpdatePlayerGuardSlider(float guard)
    {
        if (p_GuardSlider != null)
        {
            if (PlayerParameter.instance != null) p_GuardSlider.maxValue = PlayerParameter.instance.maxGuard;
            p_GuardSlider.value = guard;
        }
    }
}
