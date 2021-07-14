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
        walking,
        idle
    }


    private State state = State.idle;
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
    public float aggroRange;

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
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, collider.bounds.extents.y + 0.5f, groundLayer);
                if (hit)
                {
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x) ? hit.collider.bounds.min.x + 0.5f : hit.collider.bounds.max.x - 0.5f, transform.position.y);
                }
                return;
            case State.attacking:
                return;
            case State.walking:
                break;
            case State.idle:
                return;
        }

        Vector3 toMove = Vector3.zero;
        if (Mathf.Abs(transform.position.y - player.position.y) < yRange)
        {
            //left
            if(player.position.x < transform.position.x)
            {
                toMove = Vector2.left * speed * Time.deltaTime;
                target = new Vector2(Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer)
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
        if(Vector2.Distance(player.transform.position, transform.position) > aggroRange) { state = State.idle; return; } 
        else
        {
            state = State.walking;
        }
        if(!(state == State.walking)) { return; }
        //within attack range
        if(Mathf.Abs(transform.position.y - player.position.y) < yRange 
            && Mathf.Abs(transform.position.x - player.position.x) < attackRange)
        {

            Attack();
            return;
        }
        if (Mathf.Abs(transform.position.x - target.x) < distFromEdge 
            || Mathf.Abs(transform.position.y - player.position.y) > yRange)
        {
            FindNextPlatform();
            return;
        }
    }

    /// <summary>
    /// Update the target by evaluating nearby platforms in the direction of motion
    /// </summary>
    void FindNextPlatform()
    {
        List<Collider2D> results = new List<Collider2D>();

        float jumpAngleDelta = maxJumpAngleDeg / 5f;
        float angleToPlayer = Vector2.SignedAngle(Vector2.right, player.position - transform.position);
        for(int i = -5; i <= 5; i++)
        {
            float angle = angleToPlayer + (jumpAngleDelta * i);
            results.Add(Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)), maxJumpRange, groundLayer).collider);
        }
        //remove above and below
        results.Remove(Physics2D.Raycast(transform.position, Vector2.down, maxJumpRange, groundLayer).collider);
        results.Remove(Physics2D.Raycast(transform.position, Vector2.up, maxJumpRange, groundLayer).collider);

        //adjust to consider player position as well?
        for (int i = 0; i < results.Count; i++)
        {
            if(results[i] == null) { results.RemoveAt(i); i--; }
        }

        if(results.Count == 0) { return; }
        Vector2 closestPointToPlayer = results[0].ClosestPoint(player.position);
        Collider2D closestPlatform = results[0];
        if(results.Count > 1)
        {
            for(int i = 1; i < results.Count; i++)
            {
                if(results[i] == null) { break; }
                Vector2 temp = results[i].ClosestPoint(player.position);
                if(Vector2.Distance(temp, player.position) < Vector2.Distance(closestPointToPlayer, player.position))
                {
                    closestPointToPlayer = temp;
                    closestPlatform = results[i];
                }
            }
        }
        if(closestPlatform == null || closestPointToPlayer == null) { return; }
        Jump(closestPointToPlayer, closestPlatform);
    }

    /// <summary>
    /// Add a force to the rigidbody that will make the mummy land on the platform
    /// </summary>
    void Jump(Vector2 closestPoint, Collider2D platform)
    {
        state = State.jumping;
        Vector2 landingPoint = new Vector2(closestPoint.x, platform.bounds.max.y);
        landingPoint += (closestPoint.x < transform.position.x) ? Vector2.left : Vector2.right;

        transform.position = landingPoint + (Vector2.up * yOffset);

        /*/
        float yVel = (landingPoint.y - transform.position.y) + Physics2D.gravity.y / 2;
        float xVel = landingPoint.x - transform.position.x;

        rb.velocity = new Vector2(xVel, yVel + 1);/*/
    }

    IEnumerator Attack()
    {
        //state = State.attacking;
        return null;
    }
}
