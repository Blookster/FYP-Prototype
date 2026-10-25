using UnityEngine;
using System.Collections;

// ============================================================================
//  AttackState (enemy)
// ============================================================================
// Everything is your original code except the parts marked [NEW] / [CHANGED].
//  [FIX]     Removed "using Unity.VisualScripting.FullSerializer;" (unused).
//  [CHANGED] CheckAttackHit used to call HandleHit(15f) on the player UNCONDITIONALLY: no matter
//            where the player stood, every punch "landed". Now it first checks REACH.
//  [NEW]     IsPlayerInReach() - the VR distance check (explained below).
public class AttackState : IState
{
    private FSM fsm;
    private EnemyParameter parameter;

    public AttackState(FSM fsm)
    {
        this.fsm = fsm;
        this.parameter = fsm.parameter;
    }

    public void OnEnter()
    {
        fsm.AutoSetTimer(FighterState.Attack);
        fsm.e_CooldownManager.RetsetCooldown("AttackCooldown");

        // Debug Enter AttackState
        #region
        Debug.Log("Enter Attack State");
        #endregion

        fsm.StartCoroutine(AttackSequence());
        //parameter.attackTimer = parameter.attackCooldown;
        parameter.inCounterAttackPattern = false;
    }

    public void OnUpdate(float dist)
    {
        if (fsm.currentEnemyState == FighterState.Attack && !parameter.isAttacking)
        {
            fsm.TransitionState(FighterState.IdleWindow);

            parameter.inCounterAttackPattern = false;
            return;
        }
    }

    public void OnExit()
    {

    }

    IEnumerator AttackSequence()
    {
        parameter.isAttacking = true;

        // Phase 0: Windup Tell (Yellow Flash via Visual Manager)
        parameter.attackPhase = 0;
        if (parameter.visualManager != null) parameter.visualManager.TriggerFlash(Color.yellow, parameter.attackWindupTime);

        // Randomly pick one of the 5 punch animations!
        string[] attackTriggers = { "Jab", "RightCross", "LeftHook", "RightHook", "LeftUppercut" };
        string chosenAttack = attackTriggers[UnityEngine.Random.Range(0, attackTriggers.Length)];

        if (parameter.animator != null)
        {
            parameter.animator.SetTrigger(chosenAttack);

            Debug.Log($"[EnemyController] Throwing punch: {chosenAttack}");
        }

        yield return new WaitForSeconds(parameter.attackWindupTime);

        // Phase 1: Active Attack
        parameter.attackPhase = 1;

        Transform punchHand = parameter.useLeftPunch ? parameter.leftHand : parameter.rightHand;
        Collider punchCollider = parameter.useLeftPunch ? parameter.leftHandCollider : parameter.rightHandCollider;
        parameter.useLeftPunch = !parameter.useLeftPunch;

        if (punchHand) punchHand.localScale = Vector3.one * 1.2f;

        if (punchHand && parameter.p_fsm && parameter.p_fsm.playerParameter.headCamera)
        {
            Vector3 cameraPos = parameter.p_fsm.playerParameter.headCamera.position;
            float distance = Vector3.Distance(punchHand.position, cameraPos);
            float localExtension = Mathf.Clamp(distance - 0.2f, 0.3f, 1.8f);
            ExtendHand(punchHand, localExtension);
        }
        else
        {
            ExtendHand(punchHand, parameter.attackExtension);
        }

        if (punchCollider) punchCollider.enabled = true;

        yield return new WaitForSeconds(parameter.attackActiveTime * 0.5f);
        CheckAttackHit();
        yield return new WaitForSeconds(parameter.attackActiveTime * 0.5f);

        // Phase 2: Recovery
        parameter.attackPhase = 2;
        if (punchCollider) punchCollider.enabled = false;
        RetractHand(punchHand);
        if (punchHand) punchHand.localScale = Vector3.one;

        yield return new WaitForSeconds(parameter.attackRecoveryTime);
        parameter.isAttacking = false;
        parameter.attackPhase = 0;
    }

    // [CHANGED] ---------------------------------------------------------------------------
    void CheckAttackHit()
    {
        // Same guards as before, written as an early "return" so the real work isn't nested.
        if (parameter.p_fsm == null || !parameter.isAttacking || parameter.attackPhase != 1) return;

        if (!IsPlayerInReach())
        {
            // The punch missed because the player stepped / leaned out of range.
            Debug.Log("[Enemy] Punch whiffed - the player is out of reach.");
            return;
        }

        // The damage number now lives in EnemyParameter (Punch Damage) instead of a hard-coded 15.
        // P_FSM.HandleHit then decides what the player's state does with it (block / dodge / full hit).
        parameter.p_fsm.HandleHit(parameter.punchDamage);
    }

    // [NEW] -------------------------------------------------------------------------------
    //  THE VR DISTANCE FIX (enemy -> player half)
    // The problem: in VR you can't count on the enemy's glove physically touching the player.
    // The player is wherever they stand in their room, the glove is an animation, and there is
    // no way to "push back" a real person.
    // The fix: don't ask "did two colliders touch?". Ask "was the player's HEAD within reach
    // of this punch?" - a simple distance test. It works however the player is standing, and it
    // makes real-life movement matter: step back or lean back out of reach and the punch whiffs.
    bool IsPlayerInReach()
    {
        Transform head = null;
        if (parameter.p_fsm != null && parameter.p_fsm.playerParameter != null)
        {
            head = parameter.p_fsm.playerParameter.headCamera;
        }

        // If we can't find the head we can't measure, so fall back to the old behaviour (always hits).
        if (head == null) return true;

        Vector3 toHead = head.position - fsm.transform.position;

        // Zero the height. We measure along the FLOOR only, so a tall player or a player who
        // crouches isn't treated as "closer" or "further" from the enemy.
        toHead.y = 0.0f;

        // magnitude = length of the vector = the distance in metres.
        return toHead.magnitude <= parameter.punchReach;
    }

    void ExtendHand(Transform hand, float extension)
    {
        if (!hand) return;
        int index = (hand == parameter.leftHand) ? 0 : 1;
        hand.localPosition = parameter.originalHandPositions[index] + Vector3.forward * extension;
    }

    void RetractHand(Transform hand)
    {
        if (!hand) return;
        int index = (hand == parameter.leftHand) ? 0 : 1;
        hand.localPosition = parameter.originalHandPositions[index];
    }
}
