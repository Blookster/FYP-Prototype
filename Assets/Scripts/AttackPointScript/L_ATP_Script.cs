using UnityEngine;

// L = LEFT bubble: the fist comes in from the LEFT side and travels to the right (a left hook).
// (Same meaning as before: the old script counted a hit when the hand was on the left of the bubble.)
public class L_ATP_Script : AttackPointBase
{
    // Vector3.right = (1, 0, 0) = local +X = toward the player's right.
    protected override Vector3 RequiredPunchDirection => Vector3.right;
}
