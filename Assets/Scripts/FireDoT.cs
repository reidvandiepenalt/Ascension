using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDoT : MonoBehaviour
{
    [SerializeField] int damage, ticks;
    [SerializeField] float time;
    public List<int> instanceIDs = new List<int>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyCompositeHB hb = collision.gameObject.GetComponent<EnemyCompositeHB>();
            int instanceID = hb.ParentID;
            if (instanceIDs.Contains(instanceID)) { return; }
            instanceIDs.Add(instanceID);
            hb.healthManager.DOT(ticks, time, damage);
        }
    }
}
