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

    [SerializeField] GameObject swipePrefab, clawFacingRight, clawFacingLeft, clawUp;

    [SerializeField] float speed;
    bool facingRight = true;
    float speedMod = 1f;
    Vector2 velocity;
    Vector2 moveTarget;
    Vector2 storedJumpPoint;
    Vector2 storedJumpPointSecondHalf;
    int currentLevel = 2;

    float centerX { get => jumpPoints[JumpPoint.rightMid].x - jumpPoints[JumpPoint.leftMid].x; }

    Queue<Vector2> path = new Queue<Vector2>();
    bool reachedEndOfPath = true;

    float gravity = -80;//same as player

    [SerializeField] Transform playerTransform;
    Collider2D playerCollider;
    float playerGroundOffset;

    bool isMoving = false;
    Queue<Action> actionQ = new Queue<Action>();
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
    enum Action
    {
        walk,
        sprint,
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
            velocity.y += gravity * Time.fixedDeltaTime;
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
                case Action.walk:
                    StartCoroutine(nameof(Walk));
                    break;
                case Action.sprint:
                    StartCoroutine(nameof(Sprint));
                    break;
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

    IEnumerator Walk()
    {
        speedMod = 0.5f;
        yield return new WaitWhile(() => isMoving);
        actionQ.Dequeue();
    }

    IEnumerator Sprint()
    {
        speedMod = 1f;
        yield return new WaitWhile(() => isMoving);
        actionQ.Dequeue();
    }

    /// <summary>
    /// Claw platform attack
    /// </summary>
    IEnumerator ClawPlatform()
    {
        attacking = true;
        if (playerTransform.position.y + playerGroundOffset < jumpPoints[JumpPoint.leftMid].y)
        {
            //go to mid
            if(currentLevel == 0)
            {
                //from floor
                if(transform.position.x > centerX)
                {
                    moveTarget = jumpPoints[JumpPoint.rightFloor];
                    storedJumpPoint = jumpPoints[JumpPoint.rightMid];
                }
                else
                {
                    moveTarget = jumpPoints[JumpPoint.leftFloor];
                    storedJumpPoint = jumpPoints[JumpPoint.leftMid];
                }
            }else if (currentLevel == 2)
            {
                //from top
                if (transform.position.x > centerX)
                {
                    moveTarget = jumpPoints[JumpPoint.rightTop];
                    storedJumpPoint = jumpPoints[JumpPoint.rightWall];
                    storedJumpPointSecondHalf = jumpPoints[JumpPoint.rightMid];
                }
                else
                {
                    moveTarget = jumpPoints[JumpPoint.leftTop];
                    storedJumpPoint = jumpPoints[JumpPoint.leftWall];
                    storedJumpPointSecondHalf = jumpPoints[JumpPoint.leftMid];
                }
            }
        }
        else
        {
            //go to top
            if(currentLevel == 0)
            {
                //from floor
                if (transform.position.x > centerX)
                {
                    moveTarget = jumpPoints[JumpPoint.rightFloor];
                    storedJumpPoint = jumpPoints[JumpPoint.rightTop];
                }
                else
                {
                    moveTarget = jumpPoints[JumpPoint.leftFloor];
                    storedJumpPoint = jumpPoints[JumpPoint.leftTop];
                }
            }
            else if (currentLevel == 1)
            {
                //from middle
                if (transform.position.x > centerX)
                {
                    moveTarget = jumpPoints[JumpPoint.rightMid];
                    storedJumpPoint = jumpPoints[JumpPoint.rightWall];
                    storedJumpPointSecondHalf = jumpPoints[JumpPoint.rightTop];
                }
                else
                {
                    moveTarget = jumpPoints[JumpPoint.leftMid];
                    storedJumpPoint = jumpPoints[JumpPoint.leftWall];
                    storedJumpPointSecondHalf = jumpPoints[JumpPoint.leftTop];
                }
            }
        }
        isMoving = true;
        
        //start jump
        GeneratePath(storedJumpPoint);
        GeneratePath(storedJumpPoint, storedJumpPointSecondHalf);

        yield return new WaitUntil(() => reachedEndOfPath);
        
        moveTarget = new Vector2(playerTransform.position.x, transform.position.y);

        yield return new WaitWhile(() => isMoving);

        //claw down

        Bounds platform = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer).collider.bounds;
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

        //if(anim is done)

        //end anim

        actionQ.Dequeue();

        attacking = false;
    }

    /// <summary>
    /// Backflip move
    /// </summary>
    IEnumerator Backflip()
    {
        attacking = true;

        //start anim

        //generate path
        GeneratePath(new Vector2(transform.position.x + (facingRight ? -8 : 8), transform.position.y));

        yield return new WaitUntil(() => reachedEndOfPath);
        
        if(phase == Phase.two)
        {
            //start stomp/spike anim   adjust spawn to be at paw?
            Bounds platform = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer).collider.bounds;
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
        attacking = true;
        //start anim base on p1 or p2

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
        attacking = true;
        switch (phase)
        {
            case Phase.one:
                //start anim
                
                //wait until(Anim is done)
                actionQ.Dequeue();
                break;
            case Phase.two:
                //start eye flash anim

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

                yield return new WaitWhile(() => isMoving);

                //disable after effect

                //start claw anim

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
        attacking = true;
        switch (phase)
        {
            case Phase.one:
                
                //start anim (blur behind?)

                //set target and speed
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundLayer);
                if (hit)
                {
                    if (playerTransform.position.x < transform.position.x)
                    {
                        moveTarget = new Vector2(hit.collider.bounds.min.x + 4, transform.position.y);
                    }
                    else
                    {
                        moveTarget = new Vector2(hit.collider.bounds.max.x - 4, transform.position.y);
                    }

                }
                speedMod = 4f;

                yield return new WaitWhile(() => isMoving);

                //stop anim

                actionQ.Dequeue();
                
                break;
            case Phase.two:
                //zig zag attack


                break;
        }
        attacking = false;
    }


    IEnumerator NavigateTo(Vector2 navTarget)
    {
        JumpPoint jumpStart;
        JumpPoint jumpEnd;
        bool leftOfCenter = transform.position.x < centerX;

        //same level
        if (Mathf.Abs(navTarget.y - transform.position.y) < 1)
        {
            speedMod = 1f;
            moveTarget.x = navTarget.x;
        }
        else //move to a jump point
        {
            //determine closest jump point
            if(leftOfCenter) //left side
            {
                if(Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.leftFloor].y) < 1)//on floor
                {
                    moveTarget.x = jumpPoints[JumpPoint.leftFloor].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.leftFloor;
                }
                else if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.leftMid].y) < 1)//on mid
                {
                    moveTarget.x = jumpPoints[JumpPoint.leftMid].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.leftMid;
                }
                else //on top
                {
                    moveTarget.x = jumpPoints[JumpPoint.leftTop].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.leftTop;
                }
            }
            else //right side
            {
                if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.rightFloor].y) < 1)//on floor
                {
                    moveTarget.x = jumpPoints[JumpPoint.rightFloor].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.rightFloor;
                }
                else if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.rightMid].y) < 1)//on mid
                {
                    moveTarget.x = jumpPoints[JumpPoint.rightMid].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.rightMid;
                }
                else //on top
                {
                    moveTarget.x = jumpPoints[JumpPoint.rightTop].x;
                    moveTarget.y = transform.position.y;
                    jumpStart = JumpPoint.rightTop;
                }
            }


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

            yield return new WaitWhile(() => isMoving);

            if(jumpStart == JumpPoint.leftFloor || jumpStart == JumpPoint.rightFloor)
            {
                GeneratePath(moveTarget, jumpPoints[jumpEnd]);
            }
            else
            {
                if (leftOfCenter)
                {
                    GeneratePath(moveTarget, jumpPoints[JumpPoint.leftMid]);
                    GeneratePath(jumpPoints[JumpPoint.leftMid], jumpPoints[jumpEnd]);
                }
                else
                {
                    GeneratePath(moveTarget, jumpPoints[JumpPoint.rightMid]);
                    GeneratePath(jumpPoints[JumpPoint.rightMid], jumpPoints[jumpEnd]);
                }
            }
        }

        yield return new WaitUntil(() => reachedEndOfPath);

        moveTarget.x = navTarget.x;
        moveTarget.y = transform.position.y;

        yield return new WaitWhile(() => isMoving);
    }

    /// <summary>
    /// Checks x distance for being close enough to snap to target
    /// </summary>
    /// <returns>True if snapped to target x</returns>
    bool CheckXDistToMoveTarget()
    {
        if(Mathf.Abs(moveTarget.y - transform.position.y) < 0.5f)
        {
            if (moveTarget.x - transform.position.x < speed * Time.fixedDeltaTime * speedMod * velocity.x)
            {
                transform.position = new Vector3(moveTarget.x, transform.position.y, transform.position.z);
                velocity.x = 0;
                isMoving = false;
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
        isMoving = velocity.x != 0;
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
                actionQ.Enqueue(Action.sprint);
                actionQ.Enqueue(Action.clawSwipe);
                break;
            case 2:
                actionQ.Enqueue(Action.walk);
                actionQ.Enqueue(Action.clawSwipe);
                break;
            case 3:
                actionQ.Enqueue(Action.backflip);
                actionQ.Enqueue(Action.charge);
                break;
            case 4:
                actionQ.Enqueue(Action.walk);
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
        reachedEndOfPath = false;
        float timeToMove = 0.3f;
        int numSteps = (int)(60f * timeToMove);
        float stepDist = (endPoint.x - startPoint.x) / numSteps;

        float initVy = (endPoint.y - transform.position.y - gravity / 2 * Mathf.Pow(timeToMove, 2)) / timeToMove;
        for (int i = 0; i < numSteps; i++)
        {
            path.Enqueue(new Vector2(startPoint.x + i * stepDist,
                gravity / 2 * Mathf.Pow(i * Time.fixedDeltaTime, 2) + (i * Time.fixedDeltaTime * initVy) + startPoint.y));
        }
    }
}
