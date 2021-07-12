using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MummyScript : MonoBehaviour, IEnemy
{
    enum State
    {
        jumping,
        attacking,
        walking
    }


    private State state = State.walking;
    public Transform player;
    private Vector2 target;
    public LayerMask groundLayer;
    private ContactFilter2D filter;
    private Rigidbody2D rb;
    private Collider2D collider;
    public float attackRange = 10f;
    public float yRange = 5f;
    public float distFromEdge = 3;
    public float speed = 10;
    public float maxJumpRange = 25f;
    public float maxJumpAngleDeg = 40f;
    private float yOffset;
    public GameObject enemyGFX;

    public int Health { get; set; }
    public int MaxHealth { get; set; }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if(Health < 0)
        {
            Destroy(gameObject);
        }
    }

    public void Stun(float time)
    {
        
    }

    void Start()
    {
        Health = MaxHealth;
        filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        yOffset = rb.centerOfMass.y;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (hit)
        {
            target = new Vector2((player.position.x < transform.position.x)?hit.collider.bounds.min.x + 0.5f:
                hit.collider.bounds.max.x - 0.5f, transform.position.y);
        }

        InvokeRepeating("CheckDist", 0.5f, 0.25f);
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        switch (state)
        {
            case State.jumping:
                //check if landed
                if(Physics2D.Raycast(transform.position, Vector2.down, collider.bounds.extents.y + 0.5f, groundLayer))
                {
                    state = State.walking;
                }
                return;
            case State.attacking:
                return;
            case State.walking:
                break;
        }

        Vector3 toMove = Vector3.zero;
        if (Mathf.Abs(transform.position.y - player.position.y) < yRange)
        {
            //left
            if(player.position.x < transform.position.x)
            {
                toMove = Vector2.left * speed * Time.deltaTime;
                target = new Vector2(Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer)
                    .collider.bounds.min.x + 0.5f, transform.position.y);
                if((toMove + transform.position).x < target.x)
                {
                    FindNextPlatform();
                    return;
                }
            }
            //right
            else
            {
                toMove = Vector2.right * speed * Time.deltaTime;
                target = new Vector2(Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer)
                    .collider.bounds.max.x - 0.5f, transform.position.y);
                if ((toMove + transform.position).x > target.x)
                {
                    FindNextPlatform();
                    return;
                }
            }
            transform.position += toMove;
        }
        else
        {
            Vector2 direction = (target.x < transform.position.x) ? Vector2.left : Vector2.right;
            toMove = direction * speed * Time.deltaTime;
            transform.position += toMove;
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
    /// Checks the distance between mummy and target and mummy and player
    /// </summary>
    void CheckDist()
    {
        //within attack range
        if(Mathf.Abs(transform.position.y - player.position.y) < yRange 
            && Mathf.Abs(transform.position.x - player.position.x) < attackRange)
        {

            Attack();
            return;
        }

        //close to edge
        if(Mathf.Abs(transform.position.x - target.x) < distFromEdge)
        {
            FindNextPlatform();
        }
    }

    /// <summary>
    /// Update the target by evaluating nearby platforms in the direction of motion
    /// </summary>
    void FindNextPlatform()
    {
        List<Collider2D> results = new List<Collider2D>();

        float jumpAngleDelta = maxJumpAngleDeg / 5;
        float angleToPlayer = Vector2.SignedAngle(Vector2.right, player.position);
        for(int i = -5; i <= 5; i++)
        {
            float angle = angleToPlayer + (jumpAngleDelta * i);
            results.Add(Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) + transform.position.x,
                Mathf.Sin(angle * Mathf.Deg2Rad) + transform.position.y), maxJumpRange, groundLayer).collider);
        }

        //adjust to consider player position as well?
        int total = results.Count;
        if(total == 0) { return; }
        Vector2 closestPoint = results[0].ClosestPoint(transform.position);
        Collider2D closestPlatform = results[0];
        if(total > 1)
        {
            for(int i = 1; i < results.Count; i++)
            {
                Vector2 temp = results[i].ClosestPoint(transform.position);
                if(Vector2.Distance(temp, transform.position) < Vector2.Distance(closestPoint, transform.position))
                {
                    closestPoint = temp;
                    closestPlatform = results[i];
                }
            }
        }
        Jump(closestPoint, closestPlatform);
        target = new Vector2((player.transform.position.x < transform.position.x)
            ? closestPlatform.bounds.min.x + 0.5f : closestPlatform.bounds.max.x - 0.5f, closestPlatform.bounds.max.y + yOffset);
    }

    /// <summary>
    /// Add a force to the rigidbody that will make the mummy land on the platform
    /// </summary>
    void Jump(Vector2 closestPoint, Collider2D platform)
    {
        state = State.jumping;
        Vector2 landingPoint = new Vector2(closestPoint.x, platform.bounds.max.y);
        landingPoint += (closestPoint.x < transform.position.x) ? Vector2.left : Vector2.right;

        float yVel = (landingPoint.y - transform.position.y) + Physics2D.gravity.y / 2;
        float xVel = landingPoint.x - transform.position.x;

        rb.velocity = new Vector2(xVel, yVel + 1);
    }

    IEnumerator Attack()
    {
        //state = State.attacking;
        return null;
    }
}
