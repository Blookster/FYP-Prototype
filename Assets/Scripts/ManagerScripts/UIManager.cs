using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TMP_Text timerText;
    public Slider p_HpSlider;
    public Slider e_HpSlider;

    private void Awake()
    {
        if (instance != null) Destroy(this);
        else if (instance == null) instance = this;
    }

    private void Start()
    {
        //Instantiate(UI, transform.position, Quaternion.identity);
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
        e_HpSlider.value = hp;
    }
}
