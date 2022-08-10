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
    public Transform endParticles;

    private bool damaging = false;
    void Start()
    {
        //rotate to aim at target
        transform.position = initPosition;
        Vector2 dif = targetPosition - initPosition;
        dif.Normalize();
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg);
        RaycastHit2D hit;
        if(hit = Physics2D.Raycast(initPosition, (targetPosition - initPosition).normalized, Mathf.Infinity, groundLayer))
        {
            endParticles.position = hit.point;
        }
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
                hit.collider.gameObject.GetComponent<PlayerTestScript>().TakeDamage(damage, false);
            }
        }
    }

    /// <summary>
    /// Sets the laser to a damaging state
    /// </summary>
    public void DamageActive()
    {
        damaging = true;
    }
}
