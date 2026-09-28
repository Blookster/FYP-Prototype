using System.Collections.Generic;
using Unity.Pipeline.HotReload;
using Unity.VisualScripting;
using UnityEngine;

public class E_CooldownManager : MonoBehaviour
{
    [Header("Enemy Blocking Cooldown")]
    [SerializeField] private float blockingCooldown;
    public float o_BlockingCooldown = 3.0f;

    [Header("Enemy Attack Cooldown")]
    [SerializeField] float attackCooldown;
    public float o_AttackCooldown = 2.0f;

    [Header("Enemy Dodge Cooldown")]
    [SerializeField] float dodgeCooldown;
    public float o_DodgeCooldown = 15.0f;


    void Start()
    {
        blockingCooldown = 0.0f;
        attackCooldown = 0.0f;
        dodgeCooldown = 0.0f;
    }

    void Update()
    {
        if (!IsBlockingAvailable())
        {
            blockingCooldown -= Time.deltaTime;
            blockingCooldown = Mathf.Clamp(blockingCooldown, -1.0f, o_BlockingCooldown);
        }

        if (!IsAttackAvailable())
        {
            attackCooldown -= Time.deltaTime;
            attackCooldown = Mathf.Clamp(attackCooldown, -1.0f, o_BlockingCooldown);
        }

        if (!IsDodgeAvailable())
        {
            dodgeCooldown -= Time.deltaTime;
            dodgeCooldown = Mathf.Clamp(dodgeCooldown, -1.0f, o_DodgeCooldown);
        }
    }

    public bool IsBlockingAvailable()
    {
        if (blockingCooldown <= 0) return true;

        else return false;
        
    }

    public bool IsAttackAvailable()
    {
        if (attackCooldown <= 0) return true;
        else return false;
        
    }

    public bool IsDodgeAvailable()
    {
        if(dodgeCooldown <= 0) return true;
        else return false;
    }

    public void RetsetCooldown(string cooldownType)
    {
        if(cooldownType == "BlockingCooldown") blockingCooldown = o_BlockingCooldown;
        else if(cooldownType == "AttackCooldown") attackCooldown = o_AttackCooldown;
        else if(cooldownType == "DodgeCooldown") dodgeCooldown = o_DodgeCooldown;
    }

    
}
