using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttackInteract : AttackInteract
{
    [SerializeField] LayerMask groundMask;
    private PlayerTestScript playerScript;
    private const float distToPlayerCenter = 4.24f;

    private void Start()
    {
        playerScript = player.GetComponent<PlayerTestScript>();
    }

    protected override void OnTriggerEnter2D(Collider2D target)
    {
        //deal damage if target collision is an enemy and increase combo
        if (target.gameObject.CompareTag("Enemy"))
        {
            EnemyCompositeHB hb = target.gameObject.GetComponent<EnemyCompositeHB>();
            if (hb == null) return;
            int instanceID = hb.ParentID;
            if (instanceIDs.Contains(instanceID)) { return; }

            //check if on other side of wall
            if (Physics2D.Raycast(player.transform.position, transform.position - player.transform.position, distToPlayerCenter, groundMask).collider != null) return; //blocked by wall/ground

            instanceIDs.Add(instanceID);
            hb.TakeDamage(attackDamage);
            playerScript.ComboInc();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    private void OnDisable()
    {
        instanceIDs.Clear();
    }
}
