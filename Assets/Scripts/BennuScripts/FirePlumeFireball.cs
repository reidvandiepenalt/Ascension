using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlumeFireball : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] FloorFire fire;
    [SerializeField] float launchVel;
    bool isSpawned = false;

    private void FixedUpdate()
    {
        if(!isSpawned) { return; }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.velocity, 1, groundLayer);
        if (hit)
        {
            End();
            return;
        }
        transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg);
    }

    public void Launch(Vector2 position, float degAng)
    {
        isSpawned = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        rb.velocity = new Vector2(Mathf.Cos(degAng * Mathf.Deg2Rad), Mathf.Sin(degAng * Mathf.Deg2Rad));
        transform.eulerAngles = new Vector3(0, 0, degAng);
    }

    void End()
    {
        fire.Begin(transform.position);
        transform.position = new Vector3(-80, -80, transform.position.z);
        isSpawned = false;
    }
}
