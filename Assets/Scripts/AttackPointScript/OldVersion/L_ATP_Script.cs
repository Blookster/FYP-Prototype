using UnityEngine;

// L = LEFT bubble: the fist comes in from the LEFT side and travels to the right (a left hook).
// (Same meaning as before: the old script counted a hit when the hand was on the left of the bubble.)
public class L_ATP_Script : MonoBehaviour
{
    [SerializeField]
    Transform target;

    public float detectionRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand")
        {
            target = other.gameObject.transform;
            if (Vector3.Angle(transform.right, target.position - transform.position) > detectionRange)
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
