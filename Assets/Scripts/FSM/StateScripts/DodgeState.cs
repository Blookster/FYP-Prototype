using UnityEngine;

public class DodgeState : IState
{
    private FSM fsm;
    private Parameter parameter;

    public DodgeState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        parameter.animator.SetBool("Dodge", true);

        // Debug Enter DodgeState
        #region 
        Debug.Log("Enter Dodge State");
        #endregion

        
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        parameter.animator.SetBool("Dodge", false);
    }
}
