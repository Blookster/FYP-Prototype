using UnityEngine;

public class EnemyController : MonoBehaviour
{
    Animator anim;

    private void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
    }


    public void AttackWhenTimeIsOver()
    {
        anim.SetTrigger("Attack");
    }

    public void ActivePunch()
    {
        GameManager.instance.playerHealthscript.TakeDamage(10);
        AudioManager.instance.Play(1, "hpChangeSfx", false);
    }
}
