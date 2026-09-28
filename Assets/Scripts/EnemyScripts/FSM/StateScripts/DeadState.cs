using Unity.Collections;
using UnityEngine;

public class DeadState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public DeadState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {   
        parameter.animator.SetTrigger("KO");
        
        // Debug Enter Dead State
        #region 
        Debug.Log("Enter Dead State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        
    }
}
