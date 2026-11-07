using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text timerText;
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

    public void UpdateTimerUI(int time)
    {
        timerText.text = "TImer " + time;
    }

    public void UpdatePlayerHpSlider(float hp)
    {
        p_HpSlider.value = hp;
    }

    public void UpdateEnemyHpSlider(float hp)
    {
        if (e_HpSlider != null) e_HpSlider.value = hp;
    }

    public void UpdateEnemyGuardSlider(float guard)
    {
        if (e_GuardSlider != null) e_GuardSlider.value = guard;
    }

    public void UpdatePlayerGuardSlider(float guard)
    {
        if (p_GuardSlider != null) p_GuardSlider.value = guard;
    }
}
