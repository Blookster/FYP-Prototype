using UnityEngine;

public class B_ATP_Script : MonoBehaviour
{
    public float detectRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("PlayerHand"))
        {
            if (Vector3.Angle(transform.up, other.gameObject.transform.position - transform.position) >  detectRange)
            {
                GameManager.instance.audioManager.Play(0, "hitSfx", false);
                Debug.Log("Correct direction!!!");
                GameManager.instance.attackPointManager.StartNextRound();
            }
            else
            {
                GameManager.instance.audioManager.Play(0, "missSfx", false);
                Debug.Log("Incorrect direction!!!");
            }
            
        }
    }
}
