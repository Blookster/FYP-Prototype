using UnityEngine;

// ============================================================================
//  E_HitReceiver - sits on the enemy's head / body hitboxes.
// ============================================================================
//  [CHANGED] Damage used to be a hard-coded -10. Now it asks PlayerParameter.GetCurrentDamage(),
//            so the fist-bump special (bigger gloves) really does more damage, and it drops back
//            to normal when the special ends.
//  [CHANGED] FSM.HandleHit now takes a POSITIVE damage number (see FSM.cs).
public class E_HitReceiver : MonoBehaviour
{
    [SerializeField] private bool isHeadHit;
    private FSM fsm;

    void Start()
    {
        fsm = GetComponentInParent<FSM>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Only the player's gloves can hurt the enemy.
        if (!other.CompareTag("PlayerHand")) return;

        // A dodging enemy is invulnerable (Punch-Out rule) - the punch passes through.
        if (fsm.currentEnemyState == FighterState.Dodge)
        {
            Debug.Log("Attack Miss!!! (the enemy is dodging)");
            return;
        }

        // The "? :" below is a one-line if/else: if the PlayerParameter exists use its damage,
        // otherwise fall back to 10 so a missing component can't crash a punch.
        float damage = (PlayerParameter.instance != null) ? PlayerParameter.instance.GetCurrentDamage() : 10.0f;

        fsm.HandleHit(damage, isHeadHit);

        Debug.Log("Enemy Get Hit!!! -" + damage + " HP");
    }
}
