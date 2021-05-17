using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class FlyerTest : EnemyAI
{
    protected override void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();

        InvokeRepeating("CheckDist", 0f, 1f);
    }

    /// <summary>
    /// Check distance to target position and aggro if needed
    /// </summary>
    override protected void CheckDist()
    {
        float dist = Vector2.Distance(rb.position, target.position);

        //aggro
        if (dist <= aggroDistance)
        {
            InvokeRepeating("UpdatePath", 0f, 0.5f);
        }
        //deaggro
        if (dist >= stopDistance)
        {
            CancelInvoke("UpdatePath");
        }
    }

    /// <summary>
    /// Updates the path based on target position
    /// </summary>
    override protected void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    override protected void OnPathComplete(Path p)
    {
        ///set new path when done calculating
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    protected override void FixedUpdate()
    {
        if (path == null)
        {
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        //calculate direction and move towards it
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        //force is more floaty, but could use position instead
        rb.AddForce(force);

        //distance to next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //update waypoint
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        //flip gfx
        if (force.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
        else if (force.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
    }
}
