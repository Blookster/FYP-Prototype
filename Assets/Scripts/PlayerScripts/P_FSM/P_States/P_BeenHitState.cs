using UnityEngine;
using System;

public class P_BeenHitState : P_State
{
    private P_FSM p_fsm;
    private PlayerParameter playerParameter;

    public P_BeenHitState(P_FSM p_fsm)
    {
        this.p_fsm = p_fsm;
        playerParameter = p_fsm.playerParameter;
    }

    public void OnEnter()
    {
        // Debug Enter P_BeenHitState
        #region 
        Debug.Log("Enter P_BeenHit State");
        #endregion
    }

    public void OnUpdate()
    {
        
    }

    public void OnExit()
    {
        // Debug Exit P_BeenHitState
        #region 
        Debug.Log("Exit P_BeenHit State");
        #endregion
    }
}
