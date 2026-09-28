using System;
using UnityEngine;

public class E_HitReceiver : MonoBehaviour
{
    
    [SerializeField] private bool isHeadHit;
    private FSM fsm;

    void Start()
    {
        fsm = GetComponentInParent<FSM>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand" && fsm.currentEnemyState != FighterState.Dodge)
        {
            //Vector3 hitDir = (fsm.transform.position - other.transform.position).normalized;
            fsm.HandleHit(-10.0f, isHeadHit);

            Debug.Log("Enemy Get Hit!!!");
        }
        else
        {
            Debug.Log("Attack Miss!!!");
        }
    }
}
