using Unity.VisualScripting;
using UnityEngine;

// F = FRONT bubble: punch straight forward, away from yourself (a jab / cross).
// All the logic is in AttackPointBase; this script only says which direction is correct.
// NOTE: the old "detectRange" field is gone (it meant different things in each script).
// Use "Max Angle" from the base class instead; it's in degrees for every bubble.
public class F_ATP_Script : MonoBehaviour
{
    [SerializeField]
    Transform target;

    public float detectionRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand")
        {
            target = other.gameObject.transform;
            if (Vector3.Angle(transform.forward, target.position - transform.position) > detectionRange)
            {
                GameManager.instance.audioManager.Play(1, "hitSfx", false);
                AttackPointManager.instance.DestroyCurrentATP();
                Debug.Log("Hit!!!");
            }
            else
            {
                GameManager.instance.audioManager.Play(1, "missSfx", false);
                Debug.Log("Miss!!!");
                return;
            }
        }
    }
}
