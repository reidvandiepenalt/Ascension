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
    private Collider2D collider;
    private Rigidbody2D rb;
    public float attackRange = 10f;
    public float yRange = 5f;
    public float distFromEdge = 3;
    public float speed = 10;
    public float maxJumpRange = 25f;
    public float maxJumpAngleDeg;
    public float jumpDistToTime = 1f;
    private float yOffset;
    public GameObject enemyGFX;
    public float aggroRange;
    private bool doFindPlatform = false;

    int horizontalRayCount;
    int verticalRayCount;

    float maxClimbAngle = 60;
    float maxDescendAngle = 60;

    float horizontalRaySpacing;
    float verticalRaySpacing;
    RaycastOrigins raycastOrigins;
    private CollisionInfo collisions;
    const float skinWidth = 0.05f;
    const float distBetweenRays = 0.2f;

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
        yOffset = collider.bounds.extents.y;

        CalculateRaySpacing();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (hit)
        {
            target = new Vector2((player.position.x < transform.position.x)?hit.collider.bounds.min.x + 0.5f:
                hit.collider.bounds.max.x - 0.5f, transform.position.y);
            collisions.platform = hit.collider;
        }

        InvokeRepeating("CheckDist", 0f, 0.25f);
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        Vector3 toMove = Vector3.zero;

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
                    collisions.platform = leftHit.collider;
                }
                else if (rightHit)
                {
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x)
                        ? rightHit.collider.bounds.min.x + 0.5f : rightHit.collider.bounds.max.x - 0.5f, transform.position.y);
                    collisions.platform = rightHit.collider;
                }
                break;
            case State.attacking:
                break;
            case State.walking:
                if (Mathf.Abs(transform.position.y - player.position.y) < yRange)
                {
                    //left
                    if (player.position.x < transform.position.x)
                    {
                        toMove = Vector2.left * speed;
                        if ((toMove + transform.position).x < target.x)
                        {
                            FindNextPlatform();
                            return;
                        }
                    }
                    //right
                    else
                    {
                        toMove = Vector2.right * speed;
                        if ((toMove + transform.position).x > target.x)
                        {
                            FindNextPlatform();
                            return;
                        }
                    }
                }
                else
                {
                    Vector2 direction = (target.x < transform.position.x) ? Vector2.left : Vector2.right;
                    toMove = direction * speed;
                }
                break;
            case State.idle:
                break;
        }

        if (doFindPlatform)
        {
            FindNextPlatform();
            doFindPlatform = false;
            return;
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

        Move(toMove * Time.deltaTime);
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

        if(Physics2D.Raycast(player.position, Vector2.down, Mathf.Infinity, groundLayer).collider.bounds.center.Equals(collisions.platform.bounds.center))
        {
            target = new Vector2(player.position.x, collisions.platform.bounds.max.y);
        }
        else
        {
            target = new Vector2((player.transform.position.x < transform.position.x)
                ? collisions.platform.bounds.min.x + 0.5f : collisions.platform.bounds.max.x - 0.5f, collisions.platform.bounds.max.y);
        }
        

        //within attack range
        /*/if(Mathf.Abs(transform.position.y - player.position.y) < yRange 
            && Mathf.Abs(transform.position.x - player.position.x) < attackRange)
        {

            Attack();
            return;
        }/*/

        if(Mathf.Abs(transform.position.y - player.position.y) < yRange
            && Mathf.Abs(transform.position.x - player.position.x) > attackRange)
        {
            target = new Vector2((player.transform.position.x < transform.position.x)
                ? collisions.platform.bounds.min.x + 0.5f : collisions.platform.bounds.max.x - 0.5f, transform.position.y);
        }

        if (Mathf.Abs(transform.position.y - player.position.y) > yRange && Mathf.Abs(transform.position.x - target.x) < distFromEdge)
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
        for(int i = -6; i <= 6; i++)
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
            landingPoint = new Vector2(platform.ClosestPoint(transform.position).x, slopeTest.point.y + (yOffset * 2) + 0.5f);
        }
        else
        {
            landingPoint = new Vector2(platform.ClosestPoint(transform.position).x, platform.bounds.max.y + (yOffset * 2) + 0.25f);
        }
        float jumpTime = Mathf.Max((Vector2.Distance(landingPoint, transform.position)) / jumpDistToTime, 0.15f);
        
        float yVel = Mathf.Clamp((landingPoint.y - collider.bounds.min.y) / jumpTime - ((Physics2D.gravity * rb.gravityScale).y * jumpTime / 2), 10f, 175);
        float xVel = (landingPoint.x - transform.position.x) / jumpTime;
        if(xVel < 0) { xVel = Mathf.Clamp(xVel, -100, -5); }
        else { xVel = Mathf.Clamp(xVel, 5, 100); }

        rb.velocity = new Vector2(xVel, yVel);
    }

    IEnumerator Attack()
    {
        //state = State.attacking;
        return null;
    }


    /// <summary>
    /// Moves the player based on given move distance
    /// </summary>
    public void Move(Vector2 moveDistance)
    {
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.moveDistanceOld = moveDistance;

        //call collision functions
        if (moveDistance.y < 0)
        {
            DescendSlope(ref moveDistance);
        }
        if (moveDistance.x != 0)
        {
            HorizontalCollisions(ref moveDistance);
        }
        if (moveDistance.y != 0)
        {
            VerticalCollisions(ref moveDistance);
        }

        //move based on collision-modified distance
        transform.Translate(moveDistance);
        //update physics engine
        Physics2D.SyncTransforms();
    }

    /// <summary>
    /// Check for collisions in the horizontal direction
    /// </summary>
    /// <param name="moveDistance">Direction being moved</param>
    void HorizontalCollisions(ref Vector2 moveDistance)
    {
        float directionX = Mathf.Sign(moveDistance.x);
        float rayLength = Mathf.Abs(moveDistance.x) + skinWidth;
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, groundLayer);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle <= maxClimbAngle)
                {
                    if (!collisions.below && i != 0)
                    {
                        break;
                    }

                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        moveDistance = collisions.moveDistanceOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveDistance.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref moveDistance, slopeAngle);
                    moveDistance.x += directionX * distanceToSlopeStart;
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    moveDistance.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        moveDistance.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDistance.x);
                    }

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }
            }
        }
    }

    /// <summary>
    /// Check for collisions in y axis
    /// </summary>
    /// <param name="moveDistance">Current move distance</param>
    void VerticalCollisions(ref Vector2 moveDistance)
    {
        float directionY = Mathf.Sign(moveDistance.y);
        float rayLength = Mathf.Abs(moveDistance.y) + skinWidth;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveDistance.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, groundLayer);

            if (hit)
            {
                moveDistance.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    moveDistance.x = moveDistance.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveDistance.x);
                }

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
                collisions.platform = hit.collider;
            }
        }

        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveDistance.x);
            rayLength = Mathf.Abs(moveDistance.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveDistance.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, groundLayer);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    moveDistance.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    /// <summary>
    /// Climbs up a slope if on one
    /// </summary>
    /// <param name="moveDistance">Current move distance</param>
    /// <param name="slopeAngle">Angle of slope</param>
    void ClimbSlope(ref Vector2 moveDistance, float slopeAngle)
    {
        float moveDistanceDirection = Mathf.Abs(moveDistance.x);
        float climbmoveDistanceY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistanceDirection;
        if (moveDistance.y <= climbmoveDistanceY)
        {
            moveDistance.y = climbmoveDistanceY;
            moveDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistanceDirection * Mathf.Sign(moveDistance.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    /// <summary>
    /// Smoothly descends a slope if on one
    /// </summary>
    /// <param name="moveDistance">Current move distance</param>
    void DescendSlope(ref Vector2 moveDistance)
    {
        float directionX = Mathf.Sign(moveDistance.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, groundLayer);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveDistance.x))
                    {
                        float moveDistanceDirection = Mathf.Abs(moveDistance.x);
                        float descendmoveDistanceY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistanceDirection;
                        moveDistance.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistanceDirection * Mathf.Sign(moveDistance.x);
                        moveDistance.y -= descendmoveDistanceY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }


    /// <summary>
    /// Update raycasts to current position
    /// </summary>
    void UpdateRaycastOrigins()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    /// <summary>
    /// Calculate spacing between collision detection rays
    /// </summary>
    void CalculateRaySpacing()
    {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / distBetweenRays);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / distBetweenRays);

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    /// <summary>
    /// Structure to hold raycast origins
    /// </summary>
    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight, bottomLeft, bottomRight;
    }

    /// <summary>
    /// Holds information about current collisions
    /// </summary>
    private struct CollisionInfo
    {
        public bool above, below, left, right, climbingSlope, descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector2 moveDistanceOld;
        public Collider2D platform;

        public void Reset()
        {
            above = below = left = right = climbingSlope = descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
