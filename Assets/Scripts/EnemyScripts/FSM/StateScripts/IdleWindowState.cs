using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class IdleWindowState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public IdleWindowState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        fsm.AutoSetTimer(FighterState.IdleWindow);

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
