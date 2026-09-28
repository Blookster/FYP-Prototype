using Unity.Collections;
using UnityEngine;

public class BeenHit_Head_State : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public BeenHit_Head_State(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {   
        fsm.AutoSetTimer(FighterState.BeenHit_Head);

        if (Random.value > 0.5f) parameter.animator.SetTrigger("HeadHitRight");
        else parameter.animator.SetTrigger("HeadHitLeft");
        
        // Debug Enter BeenHit Head State
        #region 
        Debug.Log("Enter BeenHit Head State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        
    }
}
