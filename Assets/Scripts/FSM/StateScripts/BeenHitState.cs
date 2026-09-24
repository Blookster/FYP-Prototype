using UnityEngine;

public class BeenHitState : IState
{
    private FSM enemyController;
    private EnemyParameter parameter;

    public BeenHitState(FSM enemyController)
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
