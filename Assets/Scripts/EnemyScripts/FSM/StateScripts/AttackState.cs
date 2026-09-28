using UnityEngine;
using System.Collections;
using Unity.VisualScripting.FullSerializer;

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

            //parameter.animator.SetTrigger(chosenAttack);

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

    void CheckAttackHit()
    {
        if (parameter.p_fsm != null && parameter.isAttacking && parameter.attackPhase == 1)
        {
            parameter.p_fsm.HandleHit(15f);
        }
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
