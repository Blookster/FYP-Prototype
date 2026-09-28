using UnityEngine;
using UnityEngine.UI;

public class HealthSystem : MonoBehaviour
{
    [SerializeField]
    private float currentHp;

    public float mixHp = 100.0f;

    public Slider enemyHpSlider;

    public FSM fsm;

    void Start()
    {
        currentHp = mixHp;
        fsm = this.gameObject.GetComponent<FSM>();
    }

    public void ChangeHealth(float damage)
    {
        if(IsDead()) return;

        currentHp += damage;

        currentHp = Mathf.Clamp(currentHp, 0.0f, mixHp);

        enemyHpSlider.value = currentHp;

        Debug.Log("The current HP is " + currentHp + "!!!");

        if(IsDead()) fsm.TransitionState(FighterState.Dead);
    }

    bool IsDead()
    {
        if(currentHp <= 0) return true;
        else return false;
    }

    
}