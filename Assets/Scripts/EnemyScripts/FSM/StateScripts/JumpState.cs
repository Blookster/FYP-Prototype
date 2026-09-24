using UnityEngine;

public class JumpState : IState
{
    private FSM enemyController;
    private EnemyParameter parameter;

    public JumpState(FSM enemyController)
    {
        this.enemyController = enemyController;
        this.parameter = enemyController.parameter;
    }

    public void OnEnter()
    {
        
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        
    }
}
