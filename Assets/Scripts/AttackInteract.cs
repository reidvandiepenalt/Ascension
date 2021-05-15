using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteract : MonoBehaviour
{
    public int attackDamage;
    public GameObject player;

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.gameObject.tag == "Enemy")
        {
            target.gameObject.GetComponent<EnemyAI>().TakeDamage(attackDamage);
            player.GetComponent<PlayerTestScript>().ComboInc();
        }
    }
}
