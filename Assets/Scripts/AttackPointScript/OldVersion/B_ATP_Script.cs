using UnityEngine;

// B = BOTTOM bubble: the fist comes from BELOW and travels upward (an uppercut).
public class B_ATP_Script : MonoBehaviour
{
    [SerializeField]
    Transform target;

    public float detectionRange;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand")
        {
            target = other.gameObject.transform;
            if (Vector3.Angle(transform.up, target.position - transform.position) > detectionRange)
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
