using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteract : MonoBehaviour
{
    public int attackDamage;
    public GameObject player;

    void OnTriggerEnter2D(Collider2D target)
    {
        //deal damage if target collision is an enemy and increase combo
        if (target.gameObject.tag == "Enemy")
        {
            target.gameObject.GetComponent<IEnemy>().TakeDamage(attackDamage);
            player.GetComponent<PlayerTestScript>().ComboInc();
        }
    }
}
