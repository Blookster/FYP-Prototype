using UnityEngine;
using System;

public class P_AttackState : P_State
{
    private P_FSM p_fsm;
    private PlayerParameter playerParameter;

    public P_AttackState(P_FSM p_fsm)
    {
        this.p_fsm = p_fsm;
        playerParameter = p_fsm.playerParameter;
    }

    public void OnEnter()
    {
        // Debug Enter P_AttackState
        #region 
        Debug.Log("Enter P_Attack State");
        #endregion
    }

    public void OnUpdate()
    {
        p_fsm.ReturnToFixedPosition();
        p_fsm.ReturnHandsToRest();
        if (!p_fsm.CheckArmExtension())
        {
            p_fsm.P_TransitionState(P_FSM.P_FighterState.Idle);
        }

    }

    public void OnExit()
    {
        // Debug Exit AttackState
        #region 
        Debug.Log("Exit P_Attack State");
        #endregion
    }
}
