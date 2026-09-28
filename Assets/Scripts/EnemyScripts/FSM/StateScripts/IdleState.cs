using UnityEngine;
using System;

public class IdleState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;
    private P_FSM p_fsm;

    public IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
        this.p_fsm = fsm.parameter.p_fsm;
    }

    public void OnEnter()
    {
        //parameter.animator.Play("Idle");

        // Debug Enter IdleState
        #region 
        Debug.Log("Enter Idle State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        parameter.decisionTimer -= 1;
        
        // parameter.attackTimer -= Time.deltaTime;

         if (parameter.decisionTimer <= 0 && fsm.currentEnemyState == FighterState.Idle)
         {
             MakeDecision(dist);
             parameter.decisionTimer = parameter.decisionInterval;
         }
    }

    public void OnExit()
    {
        // Debug Exit IdleState
        #region 
        Debug.Log("Exit Idle State");
        #endregion
    }

    void MakeDecision(float dist)
    {
        if (parameter.p_fsm.P_currentState == P_FSM.P_FighterState.Attack && fsm.currentEnemyState == FighterState.Idle)
        {
            if (UnityEngine.Random.value < 0.5f) // 50% chance to step back and dodge when you punch
            {
                fsm.TransitionState(FighterState.Dodge);
                
                return;
            }
        }

         float rand = UnityEngine.Random.value;

        if (dist <= parameter.attackRange)
        {
            if (rand < 0.5f && fsm.e_CooldownManager.IsAttackAvailable()) fsm.TransitionState(FighterState.Attack);
            
            else if (rand < 0.8f && fsm.e_CooldownManager.IsBlockingAvailable()) fsm.TransitionState(FighterState.Blocking);
            
            else if(fsm.e_CooldownManager.IsDodgeAvailable()) fsm.TransitionState(FighterState.Dodge);
            
        }

        else
        {
            return;
        }
    }
}
