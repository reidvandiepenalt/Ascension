using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MummyGraph : MonoBehaviour, IEnemy
{
    public enum State
    {
        jumping,
        attacking,
        walking,
        idle
    }

    public Seeker seeker;
    private Vector2 target;
    public Path path;
    private int currentWaypoint = 0;

    public State state = State.idle;
    public Transform player;

    public LayerMask groundLayer;
    private ContactFilter2D filter;
    private Collider2D collider;
    public float attackRange = 10f;
    public float distFromEdge = 2f;
    public float speed = 10;
    public int maxJumpAngleDeg;
    public float jumpDistToTime = 1f;
    private float yOffset;
    public GameObject enemyGFX;
    public float aggroRange;
    private float gravity;
    public Vector2 velocity = Vector2.zero;
    public float terminalVelocity;

    public Animator anim;

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

    public void Stun(float time)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        seeker = GetComponent<Seeker>();

        Health = MaxHealth;
        filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        collider = GetComponent<Collider2D>();
        yOffset = collider.bounds.extents.y;
        gravity = -80;//same as player
        terminalVelocity = gravity * 20;

        seeker.pathCallback += OnPathComplete;
        seeker.StartPath(transform.position, player.position);
        CalculateRaySpacing();

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (hit)
        {
            target = new Vector2((player.position.x < transform.position.x) ? hit.collider.bounds.min.x + 0.5f :
                hit.collider.bounds.max.x - 0.5f, transform.position.y);
            collisions.platform = hit.collider;
        }

        InvokeRepeating("CheckDist", 0f, 0.25f);
    }

    /// <summary>
    /// generated a path from the seeker
    /// </summary>
    /// <param name="p">path p</param>
    public void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    public void OnDestroy()
    {
        seeker.pathCallback -= OnPathComplete;
    }

    public void OnDisable()
    {
        seeker.pathCallback -= OnPathComplete;
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
                    anim.SetTrigger("Grounded");
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x)
                        ? leftHit.collider.bounds.min.x + 0.5f : leftHit.collider.bounds.max.x - 0.5f, transform.position.y);
                    collisions.platform = leftHit.collider;
                }
                else if (rightHit)
                {
                    anim.SetTrigger("Grounded");
                    state = State.walking;
                    target = new Vector2((player.transform.position.x < transform.position.x)
                        ? rightHit.collider.bounds.min.x + 0.5f : rightHit.collider.bounds.max.x - 0.5f, transform.position.y);
                    collisions.platform = rightHit.collider;
                }
                if (leftHit && rightHit)
                {
                    if (leftHit.point.y != rightHit.point.y)
                    {
                        collisions.descendingSlope = true;
                    }
                }
                break;
            case State.attacking:
                break;
            case State.walking:
                if(path != null)
                {
                    if(currentWaypoint < path.vectorPath.Count)
                    {
                        Vector2 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
                        velocity += dir * speed;
                    }
                }
                break;
            case State.idle:
                break;
        }

        //update gfx
        if (velocity.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (velocity.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, terminalVelocity);
        Move(velocity * Time.deltaTime);

        if (collisions.above || collisions.below)
        {
            velocity.y = 0;
        }
        if(path == null) { return; }
        if(Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]) < distFromEdge)
        {
            currentWaypoint++;
            //reached next node of air
            if(path.path[currentWaypoint].Tag == 1)
            {
                Jump();
            }
        }
    }

    /// <summary>
    /// Checks the distance between mummy and target and mummy and player
    /// </summary>
    void CheckDist()
    {
        if (Vector2.Distance(player.transform.position, transform.position) > aggroRange && state == State.walking)
        {
            state = State.idle;
            anim.SetBool("Walk", false);
            return;
        }
        else if (state == State.idle && Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            state = State.walking;
            anim.SetBool("Walk", true);
        }

        if (state != State.walking) { return; }

        //within attack range
        /*/if(Mathf.Abs(transform.position.y - player.position.y) < yRange 
            && Mathf.Abs(transform.position.x - player.position.x) < attackRange)
        {

            Attack();
            return;
        }/*/

        seeker.StartPath(transform.position, player.position);
    }

    /// <summary>
    /// Add velocity that will make the mummy land on the platform
    /// </summary>
    void Jump()
    {
        state = State.jumping;

        for(int i = currentWaypoint; i < path.path.Count; i++)
        {
            if(path.path[i].Tag == 0)
            {
                Vector2 landingPoint = path.vectorPath[i];
                float jumpTime = Mathf.Max((Vector2.Distance(landingPoint, transform.position)) / jumpDistToTime, 0.15f);

                float yVel = Mathf.Clamp((landingPoint.y - collider.bounds.min.y) / jumpTime - (gravity * jumpTime / 2), 10f, 150);
                float xVel = (landingPoint.x - transform.position.x) / jumpTime;
                if (xVel < 0) { xVel = Mathf.Clamp(xVel, -100, -5); }
                else { xVel = Mathf.Clamp(xVel, 5, 100); }

                velocity = new Vector2(xVel, yVel);
                anim.SetTrigger("Jump");
                return;
            }
        }
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