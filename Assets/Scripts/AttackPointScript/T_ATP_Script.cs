using UnityEngine;

// T = TOP bubble: the fist comes from ABOVE and travels downward (an overhand / downward smash).
public class T_ATP_Script : AttackPointBase
{
    // Vector3.down = (0, -1, 0) = local -Y.
    protected override Vector3 RequiredPunchDirection => Vector3.down;
}
