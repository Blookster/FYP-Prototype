using UnityEngine;

// R = RIGHT bubble: the fist comes in from the RIGHT side and travels to the left (a right hook).
public class R_ATP_Script : AttackPointBase
{
    // Vector3.left = (-1, 0, 0) = local -X = toward the player's left.
    protected override Vector3 RequiredPunchDirection => Vector3.left;
}
