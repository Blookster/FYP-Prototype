using UnityEngine;

// ============================================================================
//  P_BeenHitState (player)
// ============================================================================
// [FIX] The old version of this state was EMPTY, and nothing else ever switched the player out
// of BeenHit. So the first time the enemy landed a punch, the player would be stuck in
// "BeenHit" forever (CheckInputStates returns early while BeenHit, so blocking and dodging
// would stop working too).
// Now it is a short stagger: a timer counts down, then we go back to Idle.
//
// It ALSO acts as the player's invincibility window: P_FSM.HandleHit ignores new punches
// while the state is BeenHit, so one punch can't chain into several hits.
public class P_BeenHitState : P_State
{
    private P_FSM p_fsm;
    private PlayerParameter playerParameter;
    private float stunTimer;

    public P_BeenHitState(P_FSM p_fsm)
    {
        this.p_fsm = p_fsm;
        playerParameter = p_fsm.playerParameter;
    }

    public void OnEnter()
    {
        // Start the stagger timer from the value you set in the Inspector (PlayerParameter).
        stunTimer = playerParameter.hitStunDuration;

        // Debug Enter P_BeenHitState
        #region
        Debug.Log("Enter P_BeenHit State");
        #endregion
    }

    public void OnUpdate()
    {
        // Drift back to the standing position (e.g. if we were leaning back for a dodge).
        p_fsm.ReturnToFixedPosition();

        // Time.deltaTime = seconds since the last frame, so this counts in real seconds
        // whatever the frame rate (72 / 90 / 120 Hz on Quest).
        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0.0f)
        {
            p_fsm.P_TransitionState(P_FSM.P_FighterState.Idle);
        }
    }

    public void OnExit()
    {
        // Debug Exit P_BeenHitState
        #region
        Debug.Log("Exit P_BeenHit State");
        #endregion
    }
}
