using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWallScript : MonoBehaviour, IEnemy
{
    private int health;
    public int maxHealth;
    public GameObject parent;

    public int Health { get => health; set => health = value; }
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }

    public void Stun(float time)
    {
    }

    public void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if(health < 0)
        {
            //need to add animation to attached sprite, probably figure a better way to do this effect later
            parent.SetActive(false);
        }
    }
}
