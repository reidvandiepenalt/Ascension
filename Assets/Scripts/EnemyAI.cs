using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public abstract class EnemyAI : MonoBehaviour,  IEnemy
{
    public int health;
    public int maxHealth;
    public Transform target;

    public float speed = 200f;
    public float nextWaypointDistance = 1f;
    public float aggroDistance = 20f;
    public float stopDistance = 40f;

    protected Path path;
    protected int currentWaypoint = 0;
    protected bool reachedEndOfPath = false;

    protected Seeker seeker;
    protected Rigidbody2D rb;
    public GameObject enemyGFX;

    public int Health { get => health; set => health = value; }
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, aggroDistance);
    }

    protected virtual void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
    }

    protected abstract void CheckDist();

    protected virtual void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    protected virtual void OnPathComplete(Path p)
    {
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

        if (currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        }
        else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        Vector2 force = direction * speed * Time.deltaTime;

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (force.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        } else if (force.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
    }

    public virtual void TakeDamage(int damageAmount)
    {
        health = health - damageAmount;

        if (health < 0)
        {
            print("Killed");//die
            health = maxHealth;
        }
    }

    public virtual void Stun(float timer)
    {
        print("stunned");
    }
}
