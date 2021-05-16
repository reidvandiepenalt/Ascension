using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaDirectedLaser : MonoBehaviour
{
    public int damage;
    public Vector2 targetPosition;
    public Vector2 initPosition;
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    private bool damaging = false;
    private LineRenderer lr;
    void Start()
    {
        //sets the position of second point to that of the first collision of ground in the direction of target position
        lr = GetComponent<LineRenderer>();
        lr.SetPosition(0, initPosition);
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(transform.position, (targetPosition - (Vector2)transform.position).normalized, Mathf.Infinity, groundLayer))
        {
            if (hit.collider)
            {

                lr.SetPosition(1, hit.point);
            }
        }
        else lr.SetPosition(1, (targetPosition - (Vector2)transform.position).normalized * 5000);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if player is in laser, do damage
        if (!damaging) return;
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(transform.position, (targetPosition - (Vector2)transform.position).normalized, Mathf.Infinity, playerLayer))
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                hit.collider.gameObject.GetComponent<PlayerTestScript>().TakeDamage(damage);
            }
        }
    }

    /// <summary>
    /// Sets the laser to a damaging state
    /// </summary>
    public void DamageActive()
    {
        damaging = true;
        lr.startWidth = 1;
        lr.endWidth = 1;
    }
}
