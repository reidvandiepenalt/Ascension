using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetScript : MonoBehaviour
{
    [SerializeField] Dictionary<JumpPoint, Vector2> jumpPoints;
    [SerializeField] GameObject jumpParent;
    [SerializeField] EnemyCollisionMovementHandler movement;
    [SerializeField] EnemyHealth healthManager;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Animator anim;

    [SerializeField] GameObject swipePrefab, clawFacingRight, clawFacingLeft, clawUp, tailSwipePrefab;
    [SerializeField] Transform tailEnd;

    [SerializeField] float speed;
    bool facingRight = true;
    float speedMod = 1f;
    [SerializeField] Vector2 velocity;
    [SerializeField] Vector2 moveTarget;
    [SerializeField] Vector2 navTarget;
    Vector2 storedJumpPoint;
    Vector2 storedJumpPointSecondHalf;

    float playerLevelY { get {
            if (playerTransform.position.y < jumpPoints[JumpPoint.leftMid].y - 5)
            {
                return jumpPoints[JumpPoint.leftFloor].y;
            }
            else if (playerTransform.position.y < jumpPoints[JumpPoint.leftTop].y - 5)
            {
                return jumpPoints[JumpPoint.leftMid].y;
            }
            else
            {
                return jumpPoints[JumpPoint.leftTop].y;
            }
        }
     }

    float centerX { get => jumpPoints[JumpPoint.rightMid].x - jumpPoints[JumpPoint.leftMid].x; }

    Queue<Vector2> path = new Queue<Vector2>();
    bool reachedEndOfPath = true;

    float gravity = -80;//same as player

    [SerializeField] Transform playerTransform;
    Collider2D playerCollider;
    float playerGroundOffset;

    bool isMoving = false;
    bool IsMoving { get => isMoving; set { anim.SetBool("Running", value); isMoving = value; } }
    public Queue<Action> actionQ = new Queue<Action>();
    Phase phase = Phase.one;
    bool attacking = false;

    enum JumpPoint
    {
        leftWall,
        rightWall,
        rightFloor,
        rightMid,
        leftMid,
        rightTop,
        leftTop,
        leftFloor
    }
    public enum Action
    {
        charge,
        clawSwipe,
        tailWhip,
        clawPlatform,
        backflip
    }

    enum Phase
    {
        one,
        two
    }

    private void OnValidate()
    {
        Transform[] ts = jumpParent.GetComponentsInChildren<Transform>();
        if(ts != null)
        {
            jumpPoints = new Dictionary<JumpPoint, Vector2>(ts.Length);
            for(int i = 0; i < ts.Length; i++)
            {
                string name = ts[i].name;
                switch (name)
                {
                    case "LeftWall":
                        jumpPoints.Add(JumpPoint.leftWall, ts[i].position);
                        break;
                    case "RightWall":
                        jumpPoints.Add(JumpPoint.rightWall, ts[i].position);
                        break;
                    case "JumpPointBR":
                        jumpPoints.Add(JumpPoint.rightFloor, ts[i].position);
                        break;
                    case "JumpPointMR":
                        jumpPoints.Add(JumpPoint.rightMid, ts[i].position);
                        break;
                    case "JumpPointTR":
                        jumpPoints.Add(JumpPoint.rightTop, ts[i].position);
                        break;
                    case "JumpPointBL":
                        jumpPoints.Add(JumpPoint.leftFloor, ts[i].position);
                        break;
                    case "JumpPointML":
                        jumpPoints.Add(JumpPoint.leftMid, ts[i].position);
                        break;
                    case "JumpPointTL":
                        jumpPoints.Add(JumpPoint.leftTop, ts[i].position);
                        break;
                }
            }
        }
    }


    public void OnStun(object param)
    {
        float time = (float)param;
        //stun for a given time

    }

    public void OnHit(object parameter)
    {
        int health = (int)parameter;
        if (health < 0)
        {
            Die();
        }
        else if (health < (healthManager.MaxHealth / 2f))
        {
            phase = Phase.two;
        }
    }

    void Die()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;

        phase = Phase.one;
    }


    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        if (movement.collisions.below) { velocity.y = 0; } else
        {
            if (reachedEndOfPath)
            {
                velocity.y += gravity * Time.fixedDeltaTime;
            }
        }

        if(actionQ.Count == 0)
        {
            PickAttack();
        }

        if (!reachedEndOfPath)
        {
            if (path.Count == 0)
            {
                reachedEndOfPath = true;
            }
            else
            {
                if(path.Count == 5 && actionQ.Peek() != Action.backflip)
                {
                    anim.SetTrigger("Land");
                }
                Debug.DrawLine(transform.position, path.Peek(), Color.magenta, 5f);
                transform.position = path.Dequeue();
                if (!reachedEndOfPath)
                {
                    return;
                }
            }
        }



        HorizVelCalc();

        if (!attacking)
        {
            switch (actionQ.Peek())
            {
                case Action.charge:
                    StartCoroutine(nameof(Charge));
                    break;
                case Action.clawSwipe:
                    StartCoroutine(nameof(ClawSwipe));
                    break;
                case Action.tailWhip:
                    StartCoroutine(nameof(TailSwipe));
                    break;
                case Action.clawPlatform:
                    StartCoroutine(nameof(ClawPlatform));
                    break;
                case Action.backflip:
                    StartCoroutine(nameof(Backflip));
                    break;
            }
        }

        if (CheckXDistToMoveTarget()) { return; }
        movement.Move(Time.fixedDeltaTime * velocity);
    }


    /// <summary>
    /// Claw platform attack
    /// </summary>
    IEnumerator ClawPlatform()
    {
        Debug.Log("clawPlatform");

        attacking = true;

        navTarget.y = jumpPoints[JumpPoint.leftTop].y;
        navTarget.x = Mathf.Clamp(playerTransform.position.x,
            jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
        //add a way to update constantly?
        yield return StartCoroutine(nameof(NavigateTo));

        //claw down anim
        anim.SetTrigger("Stomp");

        Bounds platform = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, Mathf.Infinity, groundLayer).collider.bounds;
        //3 or 1 seperate claw spawns (adjust to spawn at paw?)
        for (int i = 0; i < ((phase == Phase.one)?1:3); i++)
        {
            if (i == 0)
            {
                if (facingRight)
                {
                    clawFacingRight.transform.position = new Vector3(transform.position.x,
                        platform.min.y, clawFacingRight.transform.position.z);
                    clawFacingRight.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }
                else
                {
                    clawFacingLeft.transform.position = new Vector3(transform.position.x,
                        platform.min.y, clawFacingLeft.transform.position.z);
                    clawFacingLeft.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
                }
            }
            else
            {
                if (platform.min.x < transform.position.x + i || transform.position.x + i < platform.max.x)
                {
                    clawFacingRight.transform.position = new Vector3(transform.position.x + i,
                        platform.min.y, clawFacingRight.transform.position.z);
                    clawFacingRight.transform.localScale = new Vector3(0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f);
                }
                else
                {
                    clawFacingRight.transform.position = new Vector3(-50, -50, clawFacingRight.transform.position.z);
                }
                if (platform.min.x < transform.position.x - i || transform.position.x - i < platform.max.x)
                {
                    clawFacingLeft.transform.position = new Vector3(transform.position.x - i,
                        platform.min.y, clawFacingLeft.transform.position.z);
                    clawFacingLeft.transform.localScale = new Vector3(0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f);
                }
                else
                {
                    clawFacingLeft.transform.position = new Vector3(-50, -50, clawFacingLeft.transform.position.z);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

        clawFacingRight.transform.position = new Vector3(-50, -50, clawFacingRight.transform.position.z);
        clawFacingLeft.transform.position = new Vector3(-50, -50, clawFacingLeft.transform.position.z);

        actionQ.Dequeue();

        attacking = false;
    }

    /// <summary>
    /// Backflip move
    /// </summary>
    IEnumerator Backflip()
    {
        Debug.Log("backflip");

        attacking = true;

        anim.SetTrigger("Backflip");

        Bounds platform = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, Mathf.Infinity, groundLayer).collider.bounds;

        //generate path
        GeneratePath(new Vector2(Mathf.Clamp(transform.position.x + (facingRight ? -8 : 8),
            platform.min.x, platform.max.x), transform.position.y));

        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedEndOfPath);
        
        if(phase == Phase.two)
        {
            anim.SetTrigger("Stomp");

            int directionMod = facingRight ? 1 : -1;
            for (int i = 0; i < 4; i++)
            {
                if (platform.min.x < transform.position.x + (i * directionMod) || transform.position.x + (i * directionMod) < platform.max.x)
                {
                    clawUp.transform.position = new Vector3(transform.position.x + (i * directionMod),
                        platform.max.y, clawUp.transform.position.z);
                    clawUp.transform.localScale = new Vector3(0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f);
                }
                else
                {
                    clawUp.transform.position = new Vector3(-50, -50, clawUp.transform.position.z);
                }

                yield return new WaitForSeconds(0.1f);
            }

            clawUp.transform.position = new Vector3(-50, -50, clawUp.transform.position.z);

            actionQ.Dequeue();
        }
        else
        {
            //stop anim?/wait until anim is done?

            actionQ.Dequeue();
        }
        attacking = false;
    }

    /// <summary>
    /// Tail swipe attack
    /// </summary>
    IEnumerator TailSwipe()
    {
        Debug.Log("tail");

        attacking = true;

        speedMod = 1f;
        if (transform.position.x > playerTransform.position.x)
        {
            navTarget.y = playerLevelY;
            navTarget.x = playerTransform.position.x + 0.75f;
        }
        else
        {
            navTarget.y = playerLevelY;
            navTarget.x = playerTransform.position.x - 0.75f;
        }
        yield return StartCoroutine(nameof(NavigateTo));

        switch (phase)
        {
            case Phase.one:
                anim.SetTrigger("TailSwipe");
                yield return new WaitForSeconds(25f / 60f);
                TailSwipeScript ts = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (facingRight)
                {
                    ts.direction = -1;
                }
                break;
            case Phase.two:
                anim.SetTrigger("DoubleTail");
                yield return new WaitForSeconds(25f / 60f);
                TailSwipeScript ts1 = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (facingRight)
                {
                    ts1.direction = -1;
                }
                yield return new WaitForSeconds(1f / 3f);
                TailSwipeScript ts2 = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (facingRight)
                {
                    ts2.direction = -1;
                }
                break;
        }
        

        yield return new WaitForSeconds(1f);
        //wait until anim is done

        actionQ.Dequeue();

        attacking = false;
    }

    /// <summary>
    /// Claw swipe attack
    /// </summary>
    IEnumerator ClawSwipe()
    {
        Debug.Log("claw swipe");

        attacking = true;

        speedMod = 1f;
        if (transform.position.x > playerTransform.position.x)
        {
            navTarget.y = playerLevelY;
            navTarget.x = playerTransform.position.x + 0.75f;
        }
        else
        {
            navTarget.y = playerLevelY;
            navTarget.x = playerTransform.position.x - 0.75f;
        }
        yield return StartCoroutine(nameof(NavigateTo));

        switch (phase)
        {
            case Phase.one:
                anim.SetTrigger("Claw");
                
                //wait until(Anim is done)
                actionQ.Dequeue();
                break;
            case Phase.two:
                anim.SetTrigger("EyeFlash");

                //wait until(first anim is done)

                //enable after effect

                //move quickly towards player
                speedMod = 6f;
                if(transform.position.x < playerTransform.position.x)
                {
                    moveTarget.x = playerTransform.position.x - 0.75f;
                }
                else
                {
                    moveTarget.x = playerTransform.position.x + 0.75f;
                }

                yield return new WaitWhile(() => IsMoving);

                //disable after effect

                anim.SetTrigger("Claw");
                //wait until claw anim is done

                actionQ.Dequeue();

                break;
        }
        attacking = false;
    }

    /// <summary>
    /// Charge attack
    /// </summary>
    IEnumerator Charge()
    {
        Debug.Log("charge");

        attacking = true;

        speedMod = 1f;
        if (transform.position.x < centerX) //left side
        {
            if (playerLevelY == jumpPoints[JumpPoint.leftFloor].y)// player on floor
            {
                navTarget = jumpPoints[JumpPoint.leftFloor];
            }
            else if (playerLevelY == jumpPoints[JumpPoint.leftMid].y)//player on mid
            {
                navTarget = jumpPoints[JumpPoint.leftMid];
            }
            else //on top
            {
                navTarget = jumpPoints[JumpPoint.leftTop];
            }
        }
        else //right side
        {
            if (playerLevelY == jumpPoints[JumpPoint.leftFloor].y)//on floor
            {
                navTarget = jumpPoints[JumpPoint.rightFloor];
            }
            else if (playerLevelY == jumpPoints[JumpPoint.leftMid].y)//on mid
            {
                navTarget = jumpPoints[JumpPoint.rightMid];
            }
            else //on top
            {
                navTarget = jumpPoints[JumpPoint.rightTop];
            }
        }

        yield return StartCoroutine(nameof(NavigateTo));

        //start anim (blur behind?)

        //store start point
        Vector2 start = transform.position;

        //set target and speed
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, 3f, groundLayer);
        if (hit)
        {
            if (playerTransform.position.x < transform.position.x)
            {
                moveTarget = new Vector2(hit.collider.bounds.min.x + 4, hit.collider.bounds.max.y);
            }
            else
            {
                moveTarget = new Vector2(hit.collider.bounds.max.x - 4, hit.collider.bounds.max.y);
            }

        }
        speedMod = 4f;

        yield return new WaitWhile(() => IsMoving);

        //stop anim

        if(phase == Phase.two)//go back
        {
            yield return new WaitForSeconds(0.2f);
            //start anim

            //go back
            moveTarget = start;

            yield return new WaitWhile(() => IsMoving);

            //stop anim
        }

        actionQ.Dequeue();

        attacking = false;
    }


    IEnumerator NavigateTo()
    {
        JumpPoint jumpStart;
        JumpPoint jumpEnd;
        bool leftOfCenter = transform.position.x < centerX;

        //same level
        if (Mathf.Abs(navTarget.y - transform.position.y) < 1)
        {
            speedMod = 1f;
            moveTarget = navTarget;
        }
        else //move to a jump point
        {
            //determine closest jump point
            if(leftOfCenter) //left side
            {
                if(Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.leftFloor].y) < 1)//on floor
                {
                    moveTarget = jumpPoints[JumpPoint.leftFloor];
                    jumpStart = JumpPoint.leftFloor;
                }
                else if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.leftMid].y) < 1)//on mid
                {
                    moveTarget = jumpPoints[JumpPoint.leftMid];
                    jumpStart = JumpPoint.leftMid;
                }
                else //on top
                {
                    moveTarget = jumpPoints[JumpPoint.leftTop];
                    jumpStart = JumpPoint.leftTop;
                }
            }
            else //right side
            {
                if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.rightFloor].y) < 1)//on floor
                {
                    moveTarget = jumpPoints[JumpPoint.rightFloor];
                    jumpStart = JumpPoint.rightFloor;
                }
                else if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.rightMid].y) < 1)//on mid
                {
                    moveTarget = jumpPoints[JumpPoint.rightMid];
                    jumpStart = JumpPoint.rightMid;
                }
                else //on top
                {
                    moveTarget = jumpPoints[JumpPoint.rightTop];
                    jumpStart = JumpPoint.rightTop;
                }
            }
            

            yield return new WaitWhile(() => IsMoving);

            //determine where to jump to
            if (leftOfCenter) //left side
            {
                if (Mathf.Abs(navTarget.y - jumpPoints[JumpPoint.leftFloor].y) < 1)//on floor
                {
                    jumpEnd = JumpPoint.leftFloor;
                }
                else if (Mathf.Abs(navTarget.y - jumpPoints[JumpPoint.leftMid].y) < 1)//on mid
                {
                    jumpEnd = JumpPoint.leftMid;
                }
                else //on top
                {
                    jumpEnd = JumpPoint.leftTop;
                }
            }
            else //right side
            {
                if (Mathf.Abs(navTarget.y - jumpPoints[JumpPoint.rightFloor].y) < 1)//on floor
                {
                    jumpEnd = JumpPoint.rightFloor;
                }
                else if (Mathf.Abs(navTarget.y - jumpPoints[JumpPoint.rightMid].y) < 1)//on mid
                {
                    jumpEnd = JumpPoint.rightMid;
                }
                else //on top
                {
                    jumpEnd = JumpPoint.rightTop;
                }
            }

            if(jumpStart != jumpEnd)
            {
                if (jumpStart == JumpPoint.leftFloor || jumpStart == JumpPoint.rightFloor)
                {
                    GeneratePath(moveTarget, jumpPoints[jumpEnd]);
                }
                else
                {
                    if (leftOfCenter)
                    {
                        GeneratePath(moveTarget, jumpPoints[JumpPoint.leftWall]);
                        GeneratePath(jumpPoints[JumpPoint.leftWall], jumpPoints[jumpEnd]);
                    }
                    else
                    {
                        GeneratePath(moveTarget, jumpPoints[JumpPoint.rightWall]);
                        GeneratePath(jumpPoints[JumpPoint.rightWall], jumpPoints[jumpEnd]);
                    }
                }
            }

            if (jumpPoints[jumpEnd].y > transform.position.y)
            {
                anim.SetTrigger("UpJump");
            }
            else
            {
                anim.SetTrigger("DownJump");
            }
        }

        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedEndOfPath);

        moveTarget.x = navTarget.x;
        moveTarget.y = transform.position.y;

        yield return new WaitWhile(() => IsMoving);
    }

    /// <summary>
    /// Checks x distance for being close enough to snap to target
    /// </summary>
    /// <returns>True if snapped to target x</returns>
    bool CheckXDistToMoveTarget()
    {
        if(Mathf.Abs(moveTarget.y - transform.position.y) < 3f)
        {
            if (Mathf.Abs(moveTarget.x - transform.position.x) <  2 * speed * Time.fixedDeltaTime * speedMod * velocity.x)
            {
                transform.position = new Vector3(moveTarget.x, transform.position.y, transform.position.z);
                velocity.x = 0;
                IsMoving = false;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Calculates the horizontal velocity and adjusts graphics
    /// </summary>
    void HorizVelCalc()
    {
        velocity.x = (moveTarget.x > transform.position.x)?speed * speedMod : -speed * speedMod;
        if(velocity.x < 0 && facingRight)
        {
            facingRight = false;

        }else if(velocity.x > 0 && !facingRight)
        {
            facingRight = true;
            //flip gfx
        }
        IsMoving = velocity.x != 0;
    }

    /// <summary>
    /// Picks the next action set of Bastet
    /// </summary>
    void PickAttack()
    {
        bool clawable = false;

        //only do claw platform if player not on top
        if(playerTransform.position.y + playerGroundOffset < jumpPoints[JumpPoint.leftTop].y)
        {
            clawable = true;
        }

        //pick random set of attack pattern
        int randomIndex = Random.Range(0, 6 + (clawable?2:0));
        switch (randomIndex)
        {
            case 0:
                actionQ.Enqueue(Action.backflip);
                actionQ.Enqueue(Action.tailWhip);
                break;
            case 1:
                actionQ.Enqueue(Action.clawSwipe);
                break;
            case 2:
                actionQ.Enqueue(Action.clawSwipe);
                break;
            case 3:
                actionQ.Enqueue(Action.backflip);
                actionQ.Enqueue(Action.charge);
                break;
            case 4:
                actionQ.Enqueue(Action.tailWhip);
                break;
            case 5:
                actionQ.Enqueue(Action.charge);
                break;
            case 6:
                actionQ.Enqueue(Action.clawPlatform);
                break;
            case 7:
                actionQ.Enqueue(Action.backflip);
                actionQ.Enqueue(Action.clawPlatform);
                break;
        }
    }

    /// <summary>
    /// Generates a jumping path to move through
    /// </summary>
    /// <param name="endPoint">End point of the jump</param>
    void GeneratePath(Vector2 endPoint)
    {
        GeneratePath(transform.position, endPoint);
    }

    /// <summary>
    /// Generates a path from start ot end point
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    void GeneratePath(Vector2 startPoint, Vector2 endPoint)
    {
        endPoint.y += 0.5f;
        Debug.DrawLine(startPoint, endPoint, Color.red, 5f);

        reachedEndOfPath = false;
        float timeToMove = 0.3f;
        int numSteps = (int)((1/Time.fixedDeltaTime) * timeToMove);
        float stepDist = (endPoint.x - startPoint.x) / numSteps;

        float initVy = ((endPoint.y - startPoint.y) / timeToMove) + (-gravity / 2 * timeToMove);
        for (int i = 1; i <= numSteps; i++)
        {
            path.Enqueue(new Vector2(startPoint.x + i * stepDist,
                (gravity / 2) * Mathf.Pow(i * Time.fixedDeltaTime, 2) + (i * Time.fixedDeltaTime * initVy) + startPoint.y));
        }
    }
}
