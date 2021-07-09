using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MummyScript : MonoBehaviour, IEnemy
{
    public AIPath aiPath;

    public int Health { get; set; }
    public int MaxHealth { get; set; }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if(Health < 0)
        {
            Destroy(gameObject);
        }
    }

    public void Stun(float time)
    {
        
    }

    void Start()
    {
        Health = MaxHealth;
    }

    void FixedUpdate()
    {

    }
}
