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
            int instanceID = collision.gameObject.GetInstanceID();
            if (instanceIDs.Contains(instanceID)) { return; }
            instanceIDs.Add(instanceID);
            collision.gameObject.GetComponent<EnemyCompositeHB>().healthManager.DOT(ticks, time, damage);
        }
    }
}
