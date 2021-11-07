using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireDoT : MonoBehaviour
{
    [SerializeField] int damage, ticks;
    [SerializeField] float time;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //despawn on ground/wall
        if (collision.CompareTag("Enemy"))
        {
            collision.GetComponent<EnemyHealth>().DOT(ticks, time, damage);
        }
    }
}
