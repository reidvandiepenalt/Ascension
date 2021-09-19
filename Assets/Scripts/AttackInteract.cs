using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackInteract : MonoBehaviour
{
    public int attackDamage;
    public GameObject player;
    public List<int> instanceIDs;

    void OnTriggerEnter2D(Collider2D target)
    {
        //deal damage if target collision is an enemy and increase combo
        if (target.gameObject.tag == "Enemy")
        {
            int instanceID = target.GetInstanceID();
            if (instanceIDs.Contains(instanceID)) { return; }
            instanceIDs.Add(instanceID);
            target.gameObject.GetComponent<EnemyCompositeHB>().TakeDamage(attackDamage);
            player.GetComponent<PlayerTestScript>().ComboInc();
        }
    }
}
