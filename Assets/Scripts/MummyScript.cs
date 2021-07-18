using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MummyScript : MonoBehaviour, IEnemy
{
    public enum State
    {
        jumping,
        attacking,
        walking,
        idle
    }


    public State state = State.idle;
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
    public float jumpDistToTime = 1f;
    private float yOffset;
    public GameObject enemyGFX;
    public float aggroRange;
    private bool doFindPlatform = false;

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
                RaycastHit2D leftHit = Physics2D.Raycast(new Vector2(collider.bounds.min.x, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                RaycastHit2D rightHit = Physics2D.Raycast(new Vector2(collider.bounds.max.x, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                if (leftHit)
                {
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x) 
                        ? leftHit.collider.bounds.min.x + 0.5f : leftHit.collider.bounds.max.x - 0.5f, transform.position.y);
                } else if (rightHit)
                {
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x) 
                        ? rightHit.collider.bounds.min.x + 0.5f : rightHit.collider.bounds.max.x - 0.5f, transform.position.y);
                }
                return;
            case State.attacking:
                return;
            case State.walking:
                break;
            case State.idle:
                return;
        }

        if (doFindPlatform)
        {
            FindNextPlatform();
            doFindPlatform = false;
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
                target = new Vector2(Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundLayer)
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
        if(Vector2.Distance(player.transform.position, transform.position) > aggroRange && state == State.walking)
        { 
            state = State.idle; 
            return; 
        } 
        else if(state == State.idle)
        {
            state = State.walking;
        }

        if(state != State.walking) { return; }

        //within attack range
        /*/if(Mathf.Abs(transform.position.y - player.position.y) < yRange 
            && Mathf.Abs(transform.position.x - player.position.x) < attackRange)
        {

            Attack();
            return;
        }/*/
        /*/if(Mathf.Abs(transform.position.y - player.position.y) < yRange
            && Mathf.Abs(transform.position.x - player.position.x) > attackRange)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position,
                    Vector2.down, yOffset + 0.25f, groundLayer);
            target = new Vector2((player.transform.position.x < transform.position.x) 
                ? hit.collider.bounds.min.x + 0.5f : hit.collider.bounds.max.x - 0.5f, transform.position.y);
        }/*/
        if (Mathf.Abs(transform.position.y - player.position.y) > yRange)
        {
            doFindPlatform = true;
            return;
        }
    }

    /// <summary>
    /// Update the target by evaluating nearby platforms in the direction of motion
    /// </summary>
    int FindNextPlatform()
    {
        if(state == State.jumping) { return 0; }
        List<Collider2D> results = new List<Collider2D>();

        float jumpAngleDelta = maxJumpAngleDeg / 5f;
        float angleToPlayer = Vector2.SignedAngle(Vector2.right, player.position - transform.position);
        for(int i = -5; i <= 5; i++)
        {
            float angle = angleToPlayer + (jumpAngleDelta * i);
            Collider2D temp = Physics2D.Raycast(transform.position, new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)), maxJumpRange, groundLayer).collider;
            if (!results.Contains(temp)) { results.Add(temp); }
            Debug.DrawRay(transform.position, new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)) * maxJumpRange, Color.white, 5f);
        }

        for (int i = 0; i < results.Count; i++)
        {
            if (results[i] == null) { results.RemoveAt(i); i--; }
        }


        //remove directly above
        results.Remove(Physics2D.Raycast(collider.bounds.min, Vector2.up, Mathf.Infinity, groundLayer).collider);
        results.Remove(Physics2D.Raycast(collider.bounds.max, Vector2.up, Mathf.Infinity, groundLayer).collider);
        

        if (results.Count == 0) { return 0; }
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
        if(closestPlatform == null || closestPointToPlayer == null) { return 0; }
        Jump(closestPointToPlayer, closestPlatform);
        return results.Count;
    }

    /// <summary>
    /// Add a force to the rigidbody that will make the mummy land on the platform
    /// </summary>
    void Jump(Vector2 closestPointToPlayer, Collider2D platform)
    {
        state = State.jumping;
        RaycastHit2D slopeTest = Physics2D.Raycast(new Vector2(platform.bounds.center.x, 
            platform.bounds.center.y + (platform.bounds.extents.y / 2)), Vector2.down, platform.bounds.extents.y, groundLayer);
        Vector2 landingPoint;
        if (slopeTest.distance > platform.bounds.extents.y / 4) 
        {
            landingPoint = new Vector2(platform.ClosestPoint(transform.position).x, slopeTest.point.y + (yOffset * 2) + 0.25f);
        }
        else
        {
            landingPoint = new Vector2(platform.ClosestPoint(transform.position).x, platform.bounds.max.y + (yOffset * 2) + 0.25f);
        }
        float jumpTime = Mathf.Max((Vector2.Distance(landingPoint, transform.position)) / jumpDistToTime, 0.15f);
        
        float yVel = Mathf.Clamp((landingPoint.y - collider.bounds.min.y) / jumpTime - ((rb.gravityScale * Physics2D.gravity).y * jumpTime / 2), 10f, 150);
        float xVel = Mathf.Clamp((landingPoint.x - transform.position.x) / jumpTime, 0, 150);
        if(xVel == 0) { xVel = (closestPointToPlayer.x - transform.position.x) / jumpTime; }

        rb.velocity = new Vector2(xVel, yVel);
    }

    IEnumerator Attack()
    {
        //state = State.attacking;
        return null;
    }
}
