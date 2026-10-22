using UnityEngine;

public class L_ATP_Script : MonoBehaviour
{
    public float detectRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("PlayerHand"))
        {

            Vector3 dir = other.transform.position - transform.position;

            if (Vector3.Cross(Vector3.forward, dir).y < detectRange)
            {
                //Destroy(this.gameObject);
                GameManager.instance.audioManager.Play(0, "hitSfx", false);
                Debug.Log("Correct direction!!!");
                GameManager.instance.attackPointManager.StartNextRound();

            }
            else if (Vector3.Cross(Vector3.forward, dir).y > detectRange)
            {
                GameManager.instance.audioManager.Play(0, "missSfx", false);
                Debug.Log("Incorrect direction!!!");
            }

            // Debug Direction
            Debug.Log(Vector3.Cross(Vector3.forward, dir).y);
        }
    }
}
