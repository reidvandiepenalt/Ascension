using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MummyScript : EnemyAI
{
    public LayerMask groundMask;
    public Transform player;
    public float distToChangePlatforms;
    public Collider2D platformCone;
    private ContactFilter2D filter;
    private float offset;

    /// <summary>
    /// Check distance to target position and aggro if needed
    /// </summary>
    override protected void CheckDist()
    {
        float dist = Vector2.Distance(rb.position, player.position);

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

    protected override void Start()
    {
        base.Start();
        filter.layerMask = groundMask;
        offset = GetComponent<Collider2D>().bounds.extents.y;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 8f, groundMask);
        if(player.position.x < transform.position.x)
        {
            target.position = new Vector2(hit.collider.bounds.min.x, transform.position.y);
        }
        else
        {
            target.position = new Vector2(hit.collider.bounds.max.x, transform.position.y);
        }

        InvokeRepeating("CheckDist", 0f, 1f);
    }

    protected override void FixedUpdate()
    {
        if (path == null)
        {
            return;
        }

        //reached end of path
        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        //move in the direction of the next waypoint
        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 toMove = direction * speed * Time.deltaTime;

        //force vs position?
        transform.position += (Vector3)toMove;

        //distance to next waypoint
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        //update waypoint if close to next one
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
        if(Vector2.Distance(rb.position, target.position) < distToChangePlatforms)
        {
            ChangePlatforms();
        }

        //flip based on moving left or right
        if (toMove.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-Mathf.Abs(enemyGFX.transform.localScale.x), enemyGFX.transform.localScale.y, 1f);
        }
        else if (toMove.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(Mathf.Abs(enemyGFX.transform.localScale.x), enemyGFX.transform.localScale.y, 1f);
        }
    }

    /// <summary>
    /// Causes the enemy to jump to a nearby platform in order to continue chasing player
    /// </summary>
    protected void ChangePlatforms()
    {
        Collider2D[] results = new Collider2D[0];
        if(platformCone.OverlapCollider(filter, results) > 0)
        {
            Collider2D closest = results[0];
            Vector2 closestPoint = closest.ClosestPoint(transform.position);
            if(results.Length > 1)
            {
                for (int i = 1; i < results.Length; i++)
                {
                    Vector2 colliderPoint = results[i].ClosestPoint(transform.position);
                    if (Vector2.Distance(colliderPoint, transform.position) < Vector2.Distance(closestPoint, transform.position))
                    {
                        closest = results[i];
                        closestPoint = colliderPoint;
                    }
                }
            }

            //jump to next platform
            transform.position = closestPoint + Vector2.up * (closest.bounds.size.y + offset);
        }
        
    }
}
