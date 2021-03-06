using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class MummyGraph : MonoBehaviour
{
    public enum State
    {
        jumping,
        attacking,
        walking,
        idle,
        jumpStart
    }

    public Seeker seeker;
    public Path path;
    private int currentWaypoint = 0;

    public State state = State.idle;
    public Transform player;
    float playerYOffset;

    public LayerMask groundLayer;
    private ContactFilter2D filter;
    private Collider2D collider;
    public float attackRange = 10f;
    public float distToNextWaypoint = 2f;
    public float speed = 10;
    public float jumpDistToTime = 1f;
    public GameObject enemyGFX;
    public float aggroRange;
    private float gravity;
    public Vector2 velocity = Vector2.zero;
    public float terminalVelocity;

    public bool noNextPlatform = false;

    public bool jumpFailed = false;
    public Vector2 jumpFailedMoveTo = Vector2.zero;
    public bool jumpFailedMoveRight = false;
    Collider2D jumpTargetPlatform = null;
    public Vector2 landingTarget = Vector2.zero;

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


    public void OnStun(object param)
    {
        float time = (float)param;
        throw new System.NotImplementedException();
    }

    [SerializeField] void OnHit(object param)
    {
        int health = (int)param;
        if (health < 0)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //init variable setup
        seeker = GetComponent<Seeker>();
        filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        player = GameObject.FindGameObjectWithTag("Player").transform;
        collider = GetComponent<Collider2D>();
        gravity = -80;//same as player
        terminalVelocity = -40;
        playerYOffset = player.GetComponent<Collider2D>().bounds.extents.y;

        //set delegate for path callback and start path
        seeker.pathCallback += OnPathComplete;
        seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));

        //calc for collisions
        CalculateRaySpacing();

        //initial ground test
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (hit)
        {
            collisions.platform = hit.collider;
        }

        //set checkdist to repeatedly invoke
        InvokeRepeating("CheckDist", 0f, 0.25f);
    }

    /// <summary>
    /// generated a path from the seeker
    /// </summary>
    /// <param name="p">path p</param>
    public void OnPathComplete(Path p)
    {
        //error handling
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
        //prevent stuttering on path calc finish
        for(int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            if (path.vectorPath[i].x < path.vectorPath[i + 1].x)
            {
                if (transform.position.x > path.vectorPath[i].x)
                {
                    currentWaypoint++;
                }
            }
            else
            {
                if (transform.position.x < path.vectorPath[i].x)
                {
                    currentWaypoint++;
                }
                else
                {
                    break;
                }
            }
        }
        

        noNextPlatform = false;
    }

    public void OnDestroy()
    {
        //deregister delegate
        seeker.pathCallback -= OnPathComplete;
    }

    public void OnDisable()
    {
        //deregister delegate
        seeker.pathCallback -= OnPathComplete;
    }

    void FixedUpdate()
    {
        //do nothing if paused
        if (Pause.isPaused) { return; }

        RaycastHit2D leftHit;
        RaycastHit2D rightHit;
        if (state != State.jumpStart)
        {
            //check if landed and do necessary setup if it is
            leftHit = Physics2D.Raycast(new Vector2(collider.bounds.min.x + 0.05f, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
            rightHit = Physics2D.Raycast(new Vector2(collider.bounds.max.x - 0.05f, collider.bounds.min.y),
                Vector2.down, 0.1f, groundLayer);
            if (leftHit)
            {
                collisions.below = true;
                collisions.platform = leftHit.collider;
            }else if (rightHit)
            {
                collisions.below = true;
                collisions.platform = rightHit.collider;
            }
            else
            {
                collisions.below = false;
            }
        }

        switch (state)
        {
            case State.jumping:
                if(Vector2.Distance(new Vector2(transform.position.x, collider.bounds.min.y), landingTarget) < 1f)
                {
                    velocity = new Vector2(velocity.x, Mathf.Min(velocity.y, 0));
                }

                leftHit = Physics2D.Raycast(new Vector2(collider.bounds.min.x + 0.05f, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                rightHit = Physics2D.Raycast(new Vector2(collider.bounds.max.x - 0.05f, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                if (leftHit)
                {
                    anim.SetTrigger("Grounded");
                    state = State.walking;
                    collisions.platform = leftHit.collider;
                    seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));
                    if (!collisions.platform.Equals(jumpTargetPlatform))
                    {
                        jumpFailed = true;
                    }
                    else
                    {
                        jumpFailed = false;
                        jumpFailedMoveTo = Vector2.zero;
                    }
                }
                else if (rightHit)
                {
                    anim.SetTrigger("Grounded");
                    state = State.walking;
                    collisions.platform = rightHit.collider;
                    seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));
                    if (!collisions.platform.Equals(jumpTargetPlatform))
                    {
                        jumpFailed = true;
                    }
                    else
                    {
                        jumpFailed = false;
                        jumpFailedMoveTo = Vector2.zero;
                    }
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
                    if(currentWaypoint < path.vectorPath.Count && !noNextPlatform)
                    {
                        if(jumpFailed)
                        {
                            //move toward location to jump from
                            float dir = ((jumpFailedMoveTo.x - transform.position.x) > 0) ? 1 : -1;
                            velocity.x = dir * speed;
                        }
                        else
                        {
                            //move toward next waypoint
                            float dir = ((path.vectorPath[currentWaypoint].x - transform.position.x) > 0) ? 1 : -1;
                            velocity.x = dir * speed;

                            //dont walk if next node is air
                            try
                            {
                                if (path.path[currentWaypoint + 1].Tag == 1)
                                {
                                    velocity.x = 0;
                                }
                            }
                            catch 
                            {
                                if (path.path[currentWaypoint].Tag == 1)
                                {
                                    velocity.x = 0;
                                }
                            }
                            
                        }
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

        //update y vel due to gravity
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, terminalVelocity);

        //move
        Move(velocity * Time.deltaTime);

        //grounded / ceiling
        if (collisions.above || collisions.below)
        {
            velocity.y = 0;
        }

        //check if moved to/past target jump position on failed jump
        if (jumpFailed)
        {
            if (jumpFailedMoveRight)
            {
                if(transform.position.x > jumpFailedMoveTo.x)
                {
                    Jump();
                }
            }
            else
            {
                if(transform.position.x < jumpFailedMoveTo.x)
                {
                    Jump();
                }
            }
        }

        if(path == null) { return; }
        if(currentWaypoint >= path.vectorPath.Count)
        {
            //end of path; start new one
            seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));
            return;
        }
        else if(Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]) < distToNextWaypoint)
        {
            //reached next waypoint; update
            currentWaypoint++;
            if (currentWaypoint >= path.vectorPath.Count)
            {
                seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));
                return;
            }
            //reached next node of air, need to jump
            if (path.path[currentWaypoint].Tag == 1 && state == State.walking)
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
            //outside aggro range; deaggro
            state = State.idle;
            anim.SetBool("Walk", false);
            return;
        }
        else if (state == State.idle && Vector2.Distance(player.transform.position, transform.position) < aggroRange)
        {
            //inside aggro range; aggro
            state = State.walking;
            anim.SetBool("Walk", true);
        }

        //dont do anything if not walking
        if (state != State.walking) { return; }

        //within attack range
        if((transform.position - player.position).magnitude < attackRange)
        {
            StartCoroutine(Attack());
            return;
        }

        //recalc path
        seeker.StartPath(transform.position, player.position + (Vector3.down * playerYOffset));

        //reached next node of air, need to jump
        if (path.path[currentWaypoint].Tag == 1 && state == State.walking && !jumpFailed)
        {
            Jump();
        }
    }

    void JumpStartOver()
    {
        state = State.jumping;
    }

    /// <summary>
    /// Add velocity that will make the mummy land on the platform
    /// </summary>
    void Jump()
    {
        //dont jump if in air
        if (!collisions.below) { return; }

        if(currentWaypoint + 1 >= path.path.Count) { noNextPlatform = true; velocity.x = 0; return; }

        jumpFailed = false;

        for (int i = currentWaypoint + 1; i < path.path.Count; i++)
        {
            if(path.path[i].Tag == 0)
            {
                Vector2 landingPoint = path.vectorPath[i];

                float jumpTime = 0.5f;
                float yVel = (landingPoint.y - collider.bounds.min.y) / jumpTime - (gravity * jumpTime / 2);
                float xVel = (landingPoint.x - transform.position.x) / jumpTime;

                jumpTime = Mathf.Abs((landingPoint.x - transform.position.x) / xVel);

                landingTarget = landingPoint;

                //raycast for ceilings
                if (!RaycastTrajectory(transform.position, new Vector2(xVel, yVel), 2, jumpTime))
                {
                    velocity = new Vector2(xVel, yVel + 0.5f);
                    anim.SetTrigger("Jump");
                    UpdateJumpFailMoveTo(landingPoint);
                    state = State.jumpStart;
                    Invoke("JumpStartOver", 2 * Time.fixedDeltaTime);
                    return;
                }
                else //if ceiling; try shallower angle
                {
                    int segments = 4;
                    float xLength = landingPoint.x - transform.position.x;
                    float yLength = landingPoint.y - collider.bounds.min.y;
                    float maxVx = xLength / Time.fixedDeltaTime;//move to end in 1 frame
                    float xVelStep = (maxVx - xVel) / segments;
                    
                    for (int j = 1; j <= segments / 2; j++)
                    {
                        //lower trajectory and retest
                        float testVx = xVel + (xVelStep * j);
                        float t = xLength / testVx;
                        float testVy = yLength / t + (t * gravity / 2);

                        if (!RaycastTrajectory(transform.position, new Vector2(testVx, testVy), 2, t))
                        {
                            velocity = new Vector2(testVx, testVy + 0.5f);
                            anim.SetTrigger("Jump");
                            UpdateJumpFailMoveTo(landingPoint);
                            state = State.jumpStart;
                            Invoke("JumpStartOver", 2 * Time.fixedDeltaTime);
                            return;
                        }
                    }

                    UpdateJumpFailMoveTo(landingPoint);
                    jumpFailed = true;
                }
                return;
            }
        }
        noNextPlatform = true;
        velocity.x = 0;
    }

    /// <summary>
    /// Splits the trajectory into segments and checks for obstacles
    /// </summary>
    /// <returns>Returns true if there is an obstacle in the way of the path</returns>
    bool RaycastTrajectory(Vector2 startPos, Vector2 velocity, int segments, float jumpTime)
    {
        Vector2 location = new Vector2(startPos.x, startPos.y);
        float timeStep = jumpTime / segments;

        for(int i = 1; i <= segments; i++)
        {
            Vector2 rayEnd = new Vector2(startPos.x + (velocity.x * timeStep * i),
                startPos.y + (velocity.y * timeStep * i) + (gravity / 2 * Mathf.Pow(timeStep * i, 2)));
            RaycastHit2D hit = Physics2D.Raycast(location, rayEnd - location, Vector2.Distance(rayEnd, location), groundLayer);
            Debug.DrawRay(location, rayEnd - location, Color.green, 1.5f);
            if (hit) { return true; }
            location = rayEnd;
        }
        return false;
    }

    /// <summary>
    /// Updates the moveto vector if the jump fails
    /// </summary>
    void UpdateJumpFailMoveTo(Vector2 landingPoint)
    {
        jumpTargetPlatform = Physics2D.Raycast(landingPoint, Vector2.down, 5, groundLayer).collider;
        if(jumpTargetPlatform == null)
        {
            jumpTargetPlatform = Physics2D.Raycast(landingPoint - (Vector2.right * 0.25f), Vector2.down, 5, groundLayer).collider;
            if(jumpTargetPlatform == null)
            {
                jumpTargetPlatform = Physics2D.Raycast(landingPoint + (Vector2.right * 0.25f), Vector2.down, 5, groundLayer).collider;
            }
        }

        //check if current platform can be used to jump from
        if (landingPoint.x > jumpTargetPlatform.bounds.center.x)
        {
            if(collisions.platform.bounds.max.x  > jumpTargetPlatform.bounds.max.x + 5)
            {
                jumpFailedMoveRight = true;
                jumpFailedMoveTo = new Vector2(jumpTargetPlatform.bounds.max.x + 5, transform.position.y);
                return;
            }
            //check if there is a connected platform
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(collisions.platform.bounds.max.x + 0.25f, transform.position.y + 3),
                    Vector2.down, 6, groundLayer);
                if (hit)
                {
                    //presumably not a wall
                    if(hit.collider.transform.localScale.y / hit.collider.transform.localScale.x < 4)
                    {
                        //if adjacent platform goes long enough to walk to and jump from
                        if(jumpTargetPlatform.bounds.max.x + 5 < hit.collider.bounds.max.x)
                        {
                            jumpFailedMoveRight = true;
                            jumpFailedMoveTo = new Vector2(jumpTargetPlatform.bounds.max.x + 5, transform.position.y);
                            return;
                        }
                    }
                }
            }
        }
        jumpFailedMoveRight = false;
        if (collisions.platform.bounds.min.x < jumpTargetPlatform.bounds.min.x - 5)
        {
            jumpFailedMoveTo = new Vector2(Mathf.Max(jumpTargetPlatform.bounds.min.x - 5, collisions.platform.bounds.min.x),
                transform.position.y);
            return;
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(collisions.platform.bounds.min.x - 0.25f, transform.position.y + 3),
                    Vector2.down, 6, groundLayer);
            if (hit)
            {
                //presumably not a wall
                if (hit.collider.transform.localScale.y / hit.collider.transform.localScale.x < 4)
                {
                    //if adjacent platform goes long enough to walk to and jump from
                    if (hit.collider.bounds.min.x < jumpTargetPlatform.bounds.min.x - 5)
                    {
                        jumpFailedMoveRight = false;
                        jumpFailedMoveTo = new Vector2(jumpTargetPlatform.bounds.min.x - 5, transform.position.y);
                        return;
                    }
                }
            }
        }
        //if it reaches here, target platform encompasses current platform
        //just walk off i guess lul
        jumpFailedMoveRight = landingPoint.x > transform.position.x;
        jumpFailedMoveTo = new Vector2(transform.position.x + (5 * (jumpFailedMoveRight ? 1 : -1)), transform.position.y);
    }


    /// <summary>
    /// Attack coroutine
    /// </summary>
    IEnumerator Attack()
    {
        state = State.attacking;
        Vector2 attackDir = (player.position - transform.position).normalized;
        if (attackDir.x >= 0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(-1f, 1f, 1f);
        }
        else if (attackDir.x <= -0.01f)
        {
            enemyGFX.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        anim.SetFloat("AttackX", attackDir.x);
        anim.SetFloat("AttackY", attackDir.y);
        anim.SetTrigger("Attack");
        velocity.x = 0;
        while (anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            yield return null;
        }
        state = State.idle;
        yield break;
    }


    /// <summary>
    /// Moves the enemy based on given move distance
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
