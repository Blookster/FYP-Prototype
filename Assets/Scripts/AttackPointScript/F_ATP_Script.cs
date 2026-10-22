using UnityEngine;

public class F_ATP_Script : MonoBehaviour
{
    public float detectRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("PlayerHand"))
        {

            Vector3 dir = other.transform.position - transform.position;

            if (Vector3.Dot(Vector3.forward, dir) < detectRange)
            {
                
                GameManager.instance.audioManager.Play(0, "hitSfx", false);
                Debug.Log("Correct direction!!!");
                GameManager.instance.attackPointManager.StartNextRound();

            }
            else if (Vector3.Dot(Vector3.forward, dir) > detectRange)
            {
                GameManager.instance.audioManager.Play(0, "missSfx", false);
                Debug.Log("Incorrect direction!!!");
            }

            // Debug Direction
            Debug.Log(Vector3.Dot(Vector3.forward, dir));
        }
    }
}
