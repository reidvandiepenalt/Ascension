using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TernaryAnimaBall : MonoBehaviour
{
    [SerializeField] CircleCollider2D circleCollider;
    [SerializeField] SpriteRenderer sprite;
    static float respawnTime = 2f;
    [SerializeField] int damage = 5;

    void OnTriggerEnter2D(Collider2D target)
    {
        //deal damage if target collision is an enemy and increase combo
        if (target.gameObject.CompareTag("Enemy"))
        {
            EnemyCompositeHB hb = target.gameObject.GetComponent<EnemyCompositeHB>();
            hb.TakeDamage(damage);
            circleCollider.enabled = false;
            sprite.enabled = false;
            Invoke(nameof(EnableCollision), respawnTime);
        }
    }

    void EnableCollision()
    {
        circleCollider.enabled = true;
        sprite.enabled = true;
    }
}
