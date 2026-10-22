using UnityEngine;

public class T_ATP_Script : MonoBehaviour
{
    public float detectRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("PlayerHand"))
        {

            Vector3 dir = other.transform.position - transform.position;

            if (Vector3.Dot(Vector3.forward, dir) < detectRange)
            {
                Destroy(this.gameObject);
                Debug.Log("Correct direction!!!");

            }
            else if (Vector3.Dot(Vector3.forward, dir) > detectRange)
            {
                Debug.Log("Incorrect direction!!!");
            }

            // Debug Direction
            //Debug.Log(Vector3.Dot(Vector3.forward, dir));
        }
    }
}
