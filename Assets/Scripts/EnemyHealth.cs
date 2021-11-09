using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for managing enemy health
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    bool hitThisFrame = false;
    bool stunnedThisFrame = false;
    [SerializeField] int maxHealth;
    int Health { get; set; }
    public int MaxHealth { get; set; }

    public void TakeDamage(int damage)
    {
        if (hitThisFrame) { return; }
        Health -= damage;
        BroadcastMessage("OnHit", Health);
        hitThisFrame = true;
    }

    public void Stun(float time)
    {
        if (stunnedThisFrame) { return; }
        BroadcastMessage("OnStun", time);
        stunnedThisFrame = true;
    }

    private void Start()
    {
        MaxHealth = maxHealth;
        Health = MaxHealth;
    }

    private void LateUpdate()
    {
        stunnedThisFrame = false;
        hitThisFrame = false;
    }

    public void DOT(int ticks, float time, int damage)
    {
        StartCoroutine(DamageOverTime(ticks, time, damage));
    }

    IEnumerator DamageOverTime(int ticks, float time, int damage)
    {
        //add burning visual
        for (int i = 0; i < ticks; i++)
        {
            Health -= damage;
            yield return new WaitForSeconds(time);
        }
    }
}
