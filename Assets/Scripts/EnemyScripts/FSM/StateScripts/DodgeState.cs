using UnityEngine;

public class DodgeState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public DodgeState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        parameter.animator.SetBool("IsDodge", true);
        fsm.AutoSetTimer(FighterState.Dodge);
        fsm.e_CooldownManager.RetsetCooldown("DodgeCooldown");

        // Debug Enter DodgeState
        #region 
        Debug.Log("Enter Dodge State");
        #endregion

        
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        parameter.animator.SetBool("IsDodge", false);
    }
}
