using UnityEngine;

public class L_ATP_Script : MonoBehaviour
{
    public float detectRange;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == ("PlayerHand"))
        {

            Vector3 dir = other.transform.position - transform.position;

            if (Vector3.Cross(Vector3.forward, dir).y > detectRange)
            {
                Destroy(this.gameObject);
                Debug.Log("Correct direction!!!");

            }
            else if (Vector3.Dot(Vector3.forward, dir) < detectRange)
            {
                Debug.Log("Incorrect direction!!!");
            }

            // Debug Direction
            //Debug.Log(Vector3.Cross(Vector3.forward, dir).y);
        }
    }
}
