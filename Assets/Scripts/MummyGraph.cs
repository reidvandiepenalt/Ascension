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

    public EnemyCollisionMovementHandler movement;

    public State state = State.idle;
    public Transform player;
    float playerYOffset;

    public LayerMask groundLayer;
    private ContactFilter2D filter;
    private Collider2D collider;
    public float attackRange;
    public float distToNextWaypoint;
    public float speed;
    public float jumpDistToTime;
    public GameObject enemyGFX;
    public float aggroRange;
    private float gravity;
    public Vector2 velocity = Vector2.zero;
    public float terminalVelocity;
    private float yOffset = 2.87f;
    public float nodeOffset;

    public bool noNextPlatform = false;

    public bool jumpFailed = false;
    public Vector2 jumpFailedMoveTo = Vector2.zero;
    public bool jumpFailedMoveRight = false;
    Collider2D jumpTargetPlatform = null;
    public Vector2 landingTarget = Vector2.zero;
    [SerializeField] GenerateJumpPath jumpHandler;
    bool reachedEndOfPath = true;
    Queue<Vector2> jumpPath;

    public Animator anim;

    int horizontalRayCount;
    int verticalRayCount;

    float maxClimbAngle = 60;
    float maxDescendAngle = 60;

    float horizontalRaySpacing;
    float verticalRaySpacing;
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
        seeker.StartPath(transform.position - (Vector3.up * nodeOffset), player.position + (Vector3.down * playerYOffset));

        //initial ground test
        /*RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, groundLayer);
        if (hit)
        {
            collisions.platform = hit.collider;
        }*/

        //set checkdist to repeatedly invoke
        InvokeRepeating("CheckDist", 0f, 0.5f);
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
                else
                {
                    break;
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

    public void OnEnable()
    {
        seeker.pathCallback += OnPathComplete;
    }

    void FixedUpdate()
    {
        //do nothing if paused
        if (Pause.isPaused) { return; }

        /*
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
        }*/

        switch (state)
        {
            case State.jumping:
                if (!reachedEndOfPath) { 
                    if(jumpPath.Count == 0) { 
                        reachedEndOfPath = true; 
                    } else {
                        transform.position = jumpPath.Dequeue();
                    }
                }
                else
                {
                    anim.SetBool("Grounded", true);
                    state = State.walking;
                    seeker.StartPath(transform.position - Vector3.up * nodeOffset, player.position + (Vector3.down * playerYOffset));
                }

                /*
                if(Vector2.Distance(new Vector2(transform.position.x, collider.bounds.min.y), landingTarget) < 1f)
                {
                    transform.position = new Vector3(landingTarget.x, landingTarget.y + collider.bounds.extents.y, transform.position.z);
                }

                leftHit = Physics2D.Raycast(new Vector2(collider.bounds.min.x + 0.05f, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                rightHit = Physics2D.Raycast(new Vector2(collider.bounds.max.x - 0.05f, collider.bounds.min.y),
                    Vector2.down, 0.1f, groundLayer);
                if (leftHit)
                {
                    anim.SetBool("Grounded", true);
                    state = State.walking;
                    collisions.platform = leftHit.collider;
                    seeker.StartPath(transform.position - Vector3.up * yOffset, player.position + (Vector3.down * playerYOffset));
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
                    anim.SetBool("Grounded", true);
                    state = State.walking;
                    collisions.platform = rightHit.collider;
                    seeker.StartPath(transform.position - Vector3.up * yOffset, player.position + (Vector3.down * playerYOffset));
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
                }*/
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
                            anim.SetBool("Walk", true);
                        }
                        else
                        {
                            //move toward next waypoint
                            float dir = ((path.vectorPath[currentWaypoint].x - transform.position.x) > 0) ? 1 : -1;
                            velocity.x = dir * speed;
                            anim.SetBool("Walk", true);

                            //dont walk if next node is air
                            /*try
                            {
                                if (path.path[currentWaypoint + 1].Tag == 1)
                                {
                                    velocity.x = 0;
                                    anim.SetBool("Walk", false);
                                }
                            }
                            catch 
                            {*/
                                if (path.path[currentWaypoint].Tag == 1)
                                {
                                    velocity.x = 0;
                                    anim.SetBool("Walk", false);
                                }
                            //}
                            
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
        movement.Move(Time.fixedDeltaTime * velocity);

        //grounded / ceiling
        if (movement.collisions.above || movement.collisions.below)
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
            seeker.StartPath(transform.position - Vector3.up * nodeOffset, player.position + (Vector3.down * playerYOffset));
            return;
        }
        else if(Vector2.Distance(transform.position - Vector3.up * nodeOffset, path.vectorPath[currentWaypoint]) < distToNextWaypoint * (movement.collisions.descendingSlope ? 2: 1))
        {
            //reached next waypoint; update
            currentWaypoint++;
            if (currentWaypoint >= path.vectorPath.Count)
            {
                seeker.StartPath(transform.position - Vector3.up * nodeOffset, player.position + (Vector3.down * playerYOffset));
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
        seeker.StartPath(transform.position - Vector3.up * nodeOffset, player.position + (Vector3.down * playerYOffset));

        //reached next node of air, need to jump
        if(path != null)
        {
            if (path.path[currentWaypoint].Tag == 1 && state == State.walking && !jumpFailed)
            {
                Jump();
            }
        }
    }

    void JumpStartFinished()
    {
        state = State.jumping;
    }

    /// <summary>
    /// Add velocity that will make the mummy land on the platform
    /// </summary>
    void Jump()
    {
        //dont jump if in air
        if (!movement.collisions.below) { return; }

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

                //jumpTime = Mathf.Abs((landingPoint.x - transform.position.x) / xVel);

                landingTarget = landingPoint;

                //raycast for ceilings
                if (!RaycastTrajectory(transform.position, new Vector2(xVel, yVel), 2, jumpTime))
                {
                    jumpPath = jumpHandler.GeneratePath(new Vector2(transform.position.x, transform.position.y), landingPoint + Vector2.up * yOffset, jumpTime, gravity);
                    reachedEndOfPath = false;
                    //velocity = new Vector2(xVel, yVel + 0.5f);
                    anim.SetBool("Grounded", false);
                    UpdateJumpFailMoveTo(landingPoint);
                    state = State.jumpStart;
                    Invoke("JumpStartFinished", 0.1f);
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
                            jumpPath = jumpHandler.GeneratePath(new Vector2(transform.position.x, transform.position.y), landingPoint + Vector2.up * yOffset, t, gravity);
                            reachedEndOfPath = false;
                            //velocity = new Vector2(testVx, testVy + 0.5f);
                            anim.SetBool("Grounded", false);
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
            if(movement.collisions.platform.bounds.max.x  > jumpTargetPlatform.bounds.max.x + 5)
            {
                jumpFailedMoveRight = true;
                jumpFailedMoveTo = new Vector2(jumpTargetPlatform.bounds.max.x + 5, transform.position.y);
                return;
            }
            //check if there is a connected platform
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(movement.collisions.platform.bounds.max.x + 0.25f, transform.position.y + 3),
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
        if (movement.collisions.platform.bounds.min.x < jumpTargetPlatform.bounds.min.x - 5)
        {
            jumpFailedMoveTo = new Vector2(Mathf.Max(jumpTargetPlatform.bounds.min.x - 5, movement.collisions.platform.bounds.min.x),
                transform.position.y);
            return;
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(movement.collisions.platform.bounds.min.x - 0.25f, transform.position.y + 3),
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
}
