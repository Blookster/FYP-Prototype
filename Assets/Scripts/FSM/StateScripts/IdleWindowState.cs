using UnityEngine;

public class IdleWindowState : IState
{
    private FSM enemyController;
    private Parameter parameter;

    public IdleWindowState(FSM enemyController)
    {
        this.enemyController = enemyController;
        this.parameter = enemyController.parameter;
    }

    public void OnEnter()
    {
        //parameter.animator.Play("Idle");

        // Debug Enter IdleWindowState
        #region 
        Debug.Log("Enter IdleWindow State");
        #endregion
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        
    }
}
