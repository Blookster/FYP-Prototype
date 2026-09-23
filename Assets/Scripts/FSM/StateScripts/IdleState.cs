using UnityEngine;
using System;

public class IdleState : IState
{
    private FSM fsm;
    private Parameter parameter;

    public IdleState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Idle");

        // Debug Enter IdleState
        #region 
        Debug.Log("Enter Idle State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        parameter.decisionTimer -= 1;
        parameter.attackTimer -= Time.deltaTime;

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
        if (fsm.currentEnemyState == FighterState.Idle && !parameter.inCounterAttackPattern)
        {
            if (parameter.playerController.currentState == PlayerController.FighterState.Attack && fsm.currentEnemyState == FighterState.Idle)
        {
            if (UnityEngine.Random.value < 0.5f) // 50% chance to step back and dodge when you punch
            {
                fsm.TransitionState(FighterState.Dodge);
                parameter.stateTimer = parameter.dodgeDuration;
                
                return;
            }
        }

            float rand = UnityEngine.Random.value;
            
            if (dist <= parameter.attackRange && parameter.attackTimer <= 0)
            {
                if (rand < 0.5f)
                {
                    
                    fsm.TransitionState(FighterState.Attack);

                    parameter.attackTimer = parameter.attackCooldown;
                    
                }
                else if (rand < 0.8f)
                {
                    
                    fsm.TransitionState(FighterState.Blocking);
                    parameter.stateTimer = parameter.blockDuration;
                    
                }
                else
                {
                    
                    fsm.TransitionState(FighterState.Dodge);
                    parameter.stateTimer = parameter.dodgeDuration;

                }
            }

            else
            {
                return;

                // Old version
                #region 
                // fsm.currentEnemyState = (rand < 0.6f) ? FighterState.Idle : (rand < 0.8f ? FighterState.Blocking : FighterState.Dodge);
                // if (fsm.currentEnemyState == FighterState.Blocking) parameter.stateTimer = parameter.blockDuration;
                // if (fsm.currentEnemyState == FighterState.Dodge) parameter.stateTimer = parameter.dodgeDuration;
                #endregion
            }
            
        }
    }
}
