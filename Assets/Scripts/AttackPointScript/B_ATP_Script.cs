using UnityEngine;

// B = BOTTOM bubble: the fist comes from BELOW and travels upward (an uppercut).
public class B_ATP_Script : AttackPointBase
{
    // Vector3.up = (0, 1, 0) = local +Y.
    protected override Vector3 RequiredPunchDirection => Vector3.up;

    // An uppercut from below lands on the enemy's body, so play the "stomach hit" reaction.
    public override bool HitsHead => false;
}
