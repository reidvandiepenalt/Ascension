using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaVertLaser : MonoBehaviour
{
    public int damage;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float activationTime, activeTime;
    public Transform endParticles;

    private bool damaging = false;
    private LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        //sets the position of second point to that of the first collision of ground in the direction of target position
        lr = GetComponent<LineRenderer>();
        RaycastHit2D topCheck = Physics2D.Raycast(transform.position, Vector2.up, Mathf.Infinity, groundLayer);
        if (topCheck.collider)
        {
            lr.SetPosition(0, topCheck.point);
        }
        RaycastHit2D downCheck;
        if (downCheck = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer))
        {
            if (downCheck.collider)
            {

                lr.SetPosition(1, downCheck.point);
                endParticles.position = downCheck.point;
            }
        }
        else lr.SetPosition(1, Vector2.down * 5000);
        Invoke("DamageActive", activationTime);
    }


    void FixedUpdate()
    {
        //if player is in laser, do damage
        if (!damaging) return;
        RaycastHit2D hit;
        if (hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, playerLayer))
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
        lr.startWidth = 1;
        lr.endWidth = 1;
        Destroy(gameObject, activeTime);
    }
}
