using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for properties all enemies should have
/// </summary>
public interface IEnemy
{
    int Health { get; set; }
    int MaxHealth { get; set; }

    void TakeDamage(int damage);
    void Stun(float time);

}
