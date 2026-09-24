using UnityEngine;
using System;
using Unity.VisualScripting.FullSerializer;

public class P_IdleState : P_State
{   
    private P_FSM p_fsm;
    private PlayerParameter playerParameter;

    public P_IdleState(P_FSM p_fsm)
    {
        this.p_fsm = p_fsm;
        playerParameter = p_fsm.playerParameter;
    }

    public void OnEnter()
    {
        // Debug Enter P_IdleState
        #region 
        Debug.Log("Enter P_Idle State");
        #endregion
    }

    public void OnUpdate()
    {
        p_fsm.ReturnToFixedPosition();
        p_fsm.ReturnHandsToRest();
        if (p_fsm.CheckArmExtension())
        {
            p_fsm.P_TransitionState(P_FSM.P_FighterState.Attack);
        }

    }

    public void OnExit()
    {
        // Debug Exit P_IdleState
        #region 
        Debug.Log("Exit P_Idle State");
        #endregion
    }
}
