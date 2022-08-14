using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public abstract class EnemyAI : MonoBehaviour
{
    public Transform target;

    public float speed = 200f;
    public float maxSpeed = 20f;
    public float nextWaypointDistance = 1f;
    public float aggroDistance = 20f;
    public float stopDistance = 40f;

    protected Path path;
    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath = false;

    protected Seeker seeker;
    protected Rigidbody2D rb;
    public GameObject enemyGFX;

    protected virtual void Start()
    {
        //set up
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected abstract void CheckDist();

    protected virtual void UpdatePath()
    {
        //Update path
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    protected virtual void OnPathComplete(Path p)
    {
        //get new path on completion of calculation
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    protected virtual void FixedUpdate()
    {
        if(path == null)
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

        //flip based on moving left or right
        if (toMove.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-Mathf.Abs(enemyGFX.transform.localScale.x), enemyGFX.transform.localScale.y, 1f);
        } else if (toMove.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(Mathf.Abs(enemyGFX.transform.localScale.x), enemyGFX.transform.localScale.y, 1f);
        }
    }

    /// <summary>
    /// Enemy takes damage
    /// </summary>
    /// <param name="damageAmount">Damage to take</param>
    public virtual void OnHit(object param)
    {
        int health = (int)param;

        if (health < 0)
        {
            //die
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Stuns the enemy
    /// </summary>
    /// <param name="timer">Time to stay stunned</param>
    public virtual void OnStun(object param)
    {
        float stunTime = (float)param;
    }
}
