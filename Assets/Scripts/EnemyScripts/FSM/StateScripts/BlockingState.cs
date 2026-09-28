using UnityEngine;

public class BlockingState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public BlockingState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        
        parameter.animator.SetBool("IsBlocking", true);
        fsm.AutoSetTimer(FighterState.Blocking);
        fsm.e_CooldownManager.RetsetCooldown("BlockingCooldown");

         // Debug Enter IdleState
        #region 
        Debug.Log("Enter Blocking State");
        #endregion

        RaiseHandsForBlock();
    }

    public void OnUpdate(float dist)
    {
        
    }

    public void OnExit()
    {
        parameter.animator.SetBool("IsBlocking", false);
    }

    void RaiseHandsForBlock()
    {
        float targetY = parameter.originalHandY + parameter.blockHandRaiseY;
        if (parameter.leftHand)
        {
            Vector3 pos = parameter.leftHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            parameter.leftHand.localPosition = pos;
        }
        if (parameter.rightHand)
        {
            Vector3 pos = parameter.rightHand.localPosition;
            pos.y = Mathf.Lerp(pos.y, targetY, 10f * Time.deltaTime);
            parameter.rightHand.localPosition = pos;
        }
    }
}
