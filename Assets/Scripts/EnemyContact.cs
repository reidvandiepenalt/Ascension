using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    public int contactDamage;
    public bool setToGround = false;

    void OnTriggerStay2D(Collider2D target)
    {
        //if player is in hitbox, damage it
        if (target.gameObject.CompareTag("Player"))
        {
            target.gameObject.GetComponent<PlayerTestScript>().TakeDamage(contactDamage, setToGround);
        }
    }
}
