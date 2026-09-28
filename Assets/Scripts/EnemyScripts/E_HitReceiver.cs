using System;
using UnityEngine;

public class E_HitReceiver : MonoBehaviour
{
    private FSM fsm;

    void Start()
    {
        fsm = GetComponentInParent<FSM>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PlayerHand" && fsm.currentEnemyState != FighterState.Dodge)
        {
            

            Debug.Log("Enemy Get Hit!!!");
        }
    }
}
