using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCompositeHB : MonoBehaviour
{
    public EnemyHealth healthManager;

    public int ParentID { get; private set; }

    private void Start()
    {
        ParentID = healthManager.gameObject.GetInstanceID();
    }

    public void TakeDamage(int damage)
    {
        healthManager.TakeDamage(damage);
    }

    public void Stun(float stunTime)
    {
        healthManager.Stun(stunTime);
    }
}
