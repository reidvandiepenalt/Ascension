using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteract : MonoBehaviour
{
    public int attackDamage;
    public GameObject player;
    public List<int> instanceIDs;

    protected virtual void OnTriggerEnter2D(Collider2D target)
    {
        //deal damage if target collision is an enemy and increase combo
        if (target.gameObject.CompareTag("Enemy"))
        {
            EnemyCompositeHB hb = target.gameObject.GetComponent<EnemyCompositeHB>();
            int instanceID = hb.ParentID;
            if (instanceIDs.Contains(instanceID)) { return; }
            instanceIDs.Add(instanceID);
            hb.TakeDamage(attackDamage);
            player.GetComponent<PlayerTestScript>().ComboInc();
        }
    }
}
