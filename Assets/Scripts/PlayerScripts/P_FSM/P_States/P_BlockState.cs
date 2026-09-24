using UnityEngine;
using System;

public class P_BlockState : P_State
{
    private P_FSM p_fsm;
    private PlayerParameter playerParameter;
    
    public P_BlockState(P_FSM p_fsm)
    {
        this.p_fsm = p_fsm;
        playerParameter = p_fsm.playerParameter;
    }

    public void OnEnter()
    {
        // Debug Enter P_BlockState
        #region 
        Debug.Log("Enter P_BlockState State");
        #endregion
    }

    public void OnUpdate()
    {
        p_fsm.ReturnToFixedPosition();
        p_fsm.RaiseHandsForBlock();
        if (!playerParameter.inputHandler.IsBlocking)
        {
            p_fsm.P_TransitionState(P_FSM.P_FighterState.Idle);
        }

    }

    public void OnExit()
    {
        // Debug Exit P_BlockState
        #region 
        Debug.Log("Exit P_Block State");
        #endregion
    }
}
