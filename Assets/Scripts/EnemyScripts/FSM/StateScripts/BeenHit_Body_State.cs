using UnityEngine;

public class BeenHit_Body_State : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public BeenHit_Body_State(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {   
        fsm.AutoSetTimer(FighterState.BeenHit_Body);
        
        parameter.animator.SetTrigger("StomachHit");

        // Debug Enter BeenHit Body State
        #region 
        Debug.Log("Enter BeenHit Body State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        
    }
}
