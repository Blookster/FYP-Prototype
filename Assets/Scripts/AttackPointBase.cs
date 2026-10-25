using UnityEngine;

// ============================================================================
//  AttackPointBase  -  the shared brain of every bubble (F / L / R / T / B).
// ============================================================================
// WHY THIS FILE EXISTS
// Before, F_ATP_Script, L_ATP_Script, R_ATP_Script, T_ATP_Script and B_ATP_Script each had
// their own copy of "check the tag -> check the direction -> play a sound -> next round".
// Five copies = five places to fix every bug (and they used three different ways of
// measuring direction, all in WORLD space, so turning the VR rig would break them).
//
// Now the logic lives ONCE, here. Each of the five scripts only answers one question:
// "which way must the fist travel for this bubble to count?"
//
// This is the TEMPLATE METHOD pattern: the base class runs the fixed recipe
// (OnTriggerEnter) and child classes fill in the one step that differs
// (RequiredPunchDirection).
//
// "abstract" = you can't put this script on a GameObject directly, only a child of it.
// Your existing prefabs keep working because they still use F_/L_/R_/T_/B_ATP_Script,
// which now inherit from this class.
public abstract class AttackPointBase : MonoBehaviour
{
    [Header("Punch Detection")]
    [Tooltip("How far (in degrees) the real punch direction may be from the ideal one. Smaller = stricter. 60 is forgiving, 30 is tight.")]
    [Range(10.0f, 90.0f)]
    public float maxAngle = 60.0f;

    [Tooltip("Slowest hand speed (metres/second) that still counts as a punch. Stops the bubble popping when you just rest a hand on it. Set 0 to disable.")]
    public float minPunchSpeed = 1.0f;

    [Header("Damage")]
    [Tooltip("Multiplier on top of the manager's bubble damage. Make a bubble type worth more or less.")]
    public float damageMultiplier = 1.0f;

    // ---- The two things a child class must/can supply ----------------------------------

    // The direction the fist must TRAVEL, in the bubble's own LOCAL axes:
    //   local +Z = away from the player (toward the enemy)   local +X = to the player's right
    //   local +Y = up
    // Local space matters: AttackPointManager spawns every bubble rotated to face along the
    // player's view, so "forward" always means "away from the player" no matter which way
    // they stand in their room.
    protected abstract Vector3 RequiredPunchDirection { get; }

    // Does popping this bubble hurt the enemy's head or body? (decides which hit animation plays)
    // "virtual" = has a default (head) that a child class MAY override (the bottom bubble does).
    public virtual bool HitsHead => true;

    // Once a bubble has been hit successfully it must not be hit again in the same frame
    // (two gloves touching at once would deal damage twice).
    private bool consumed = false;

    // "protected virtual" instead of private: Unity calls OnTriggerEnter on base classes
    // reliably when it's protected, and a child could still extend it if we ever need that.
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (consumed) return;

        // Only gloves count. (Same tag you were already using.)
        if (!other.CompareTag("PlayerHand")) return;

        // ---- 1. Work out which way (and how fast) the fist was really travelling --------
        // GetComponentInParent also looks at the object itself, so it works whether the
        // collider is on the glove or on a child of the glove.
        HandVelocityTracker tracker = other.GetComponentInParent<HandVelocityTracker>();
        if (tracker == null)
        {
            // First time this glove ever touched a bubble: give it a tracker so every later
            // punch is measured properly. No manual setup needed.
            tracker = other.gameObject.AddComponent<HandVelocityTracker>();
        }

        Vector3 punchDirection;
        if (tracker.IsReady)
        {
            // Too slow = not a punch. Return silently: no hit and no "miss" sound, because
            // brushing against a bubble shouldn't punish the player.
            if (tracker.Speed < minPunchSpeed) return;

            // normalized = same direction, length 1. Angle() only cares about direction.
            punchDirection = tracker.Velocity.normalized;
        }
        else
        {
            // Fallback for the very first touch (the tracker has no history yet): assume the
            // fist is heading from where it is toward the bubble's centre.
            punchDirection = (transform.position - other.transform.position).normalized;
        }

        // ---- 2. Compare it with the ideal direction -----------------------------------
        // The ideal direction is defined in the bubble's local space, so convert it to a
        // world direction (this applies the bubble's rotation) before comparing.
        Vector3 idealDirection = transform.TransformDirection(RequiredPunchDirection);

        // Angle = 0 means perfectly aligned, 90 = sideways, 180 = exactly backwards.
        float angle = Vector3.Angle(idealDirection, punchDirection);

        Debug.Log("[" + name + "] punch angle " + angle.ToString("F0") + " deg (limit " + maxAngle + "), speed " + tracker.Speed.ToString("F1") + " m/s");

        // ---- 3. Hit or miss ------------------------------------------------------------
        if (angle <= maxAngle)
        {
            consumed = true;
            // The bubble does NOT deal damage itself. It reports to the manager, which owns
            // the rules (damage amount, respawn timing, game state). One owner = one place to change.
            GameManager.instance.attackPointManager.OnAttackPointHit(this);
        }
        else
        {
            GameManager.instance.audioManager.Play(0, "missSfx", false);
        }
    }
}
