using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class PatrolEnemyTest : EnemyAI
{
    public Transform leftPatrolPoint;
    public Transform rightPatrolPoint;
    public Patrolling initDirection;
    public LayerMask layerMask;
    [SerializeField] bool usePlatformEdges;

    Patrolling patrolling;

    public enum Patrolling
    {
        left,
        right,
    }

    override protected void Start()
    {
        base.Start();

        if (usePlatformEdges)
        {
            GetPoints();
        }

        //starting direction
        if(initDirection == Patrolling.left)
        {
            target = leftPatrolPoint;
            patrolling = Patrolling.left;
        }
        else
        {
            target = rightPatrolPoint;
            patrolling = Patrolling.right;
        }
        InvokeRepeating("CheckDist", 0f, 0.1f);
        InvokeRepeating("UpdatePath", 0f, 0.2f);
    }

    /// <summary>
    /// Checks distance to target
    /// </summary>
    override protected void CheckDist()
    {
        float dist = Mathf.Abs(rb.position.x - target.position.x);

        //turn around point
        if (dist < nextWaypointDistance * 1.5f)
        {
            if (patrolling == Patrolling.left)
            {
                target = rightPatrolPoint;
                patrolling = Patrolling.right;
            }
            else
            {
                target = leftPatrolPoint;
                patrolling = Patrolling.left;
            }
        }
    }

    override protected void UpdatePath()
    {
        //update path
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    override protected void OnPathComplete(Path p)
    {
        //get new path when calculation is done
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    override protected void FixedUpdate()
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

        //move toward next waypoint
        Vector2 direction = new Vector2((patrolling == Patrolling.left) ? -1 : 1, 0);
        Vector3 toMove = direction * speed * Time.deltaTime;
        transform.position += toMove;

        //see if close enough to waypoint to switch to next
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        //update gfx
        if (toMove.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (toMove.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(1f, 1f, 1f);
        }
    }

    /// <summary>
    /// Spawn points at edges of current platform
    /// </summary>
    void GetPoints()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, layerMask);
        if (hit)
        {
            leftPatrolPoint.position = new Vector2(hit.collider.bounds.min.x + 0.05f, hit.collider.bounds.max.y);
            rightPatrolPoint.position = new Vector2(hit.collider.bounds.max.x - 0.05f, hit.collider.bounds.max.y);
        }
    }
}
