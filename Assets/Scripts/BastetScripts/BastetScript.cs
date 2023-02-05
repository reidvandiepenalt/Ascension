using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetScript : BossAI
{
    [SerializeField] Dictionary<JumpPoint, Vector2> jumpPoints;
    [SerializeField] GameObject jumpParent;
    [SerializeField] EnemyCollisionMovementHandler movement;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] GameObject swipePrefab, clawFacingRight, clawFacingLeft, clawUp, tailSwipePrefab;
    [SerializeField] Transform tailEnd;

    [SerializeField] float speed;
    bool facingRight = true;
    bool isNavigating = false;
    float speedMod = 1f;
    [SerializeField] Vector2 velocity;
    [SerializeField] Vector2 moveTarget;
    [SerializeField] Vector2 navTarget;
    Vector2 storedJumpPoint;
    Vector2 storedJumpPointSecondHalf;

    int clawAnim, tailAnim, doubleTailAnim, runningAnim, landAnim, stompAnim, backflipAnim, noTailAnim, eyeFlashAnim, blurLeftAnim, blurRightAnim, upJumpAnim, downJumpAnim;

    float playerLevelY { get {
            if (playerTransform.position.y < jumpPoints[JumpPoint.leftMid].y - 2)
            {
                return jumpPoints[JumpPoint.leftFloor].y;
            }
            else if (playerTransform.position.y < jumpPoints[JumpPoint.leftTop].y - 2)
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

    [SerializeField] BastetSFXManager sfxManager;

    bool isMoving = false;
    bool IsMoving { get => isMoving; set { anim.SetBool(runningAnim, value); isMoving = value; } }
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


    public override void OnStun(object param)
    {
        float time = (float)param;
        //stun for a given time

    }

    public override void OnHit(object parameter)
    {
        sfxManager.PlayHurt();
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

    protected override void Die()
    {
        //implement

        Destroy(gameObject);

        item.transform.position = new Vector3(35, 14, 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        runningAnim = Animator.StringToHash("Running");
        landAnim = Animator.StringToHash("Land");
        stompAnim = Animator.StringToHash("Stomp");
        backflipAnim = Animator.StringToHash("Backflip");
        tailAnim = Animator.StringToHash("TailSwipe");
        doubleTailAnim = Animator.StringToHash("DoubleTail");
        noTailAnim = Animator.StringToHash("NoTail");
        clawAnim = Animator.StringToHash("Claw");
        eyeFlashAnim = Animator.StringToHash("EyeFlash");
        blurRightAnim = Animator.StringToHash("BlurRight");
        blurLeftAnim = Animator.StringToHash("BlurLeft");
        upJumpAnim = Animator.StringToHash("UpJump");
        downJumpAnim = Animator.StringToHash("DownJump");

        groundLayer = LayerMask.GetMask("Ground");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;

        phase = Phase.one;
    }


    void FixedUpdate()
    {
        if (Pause.isPaused || !AIisActive) { return; }

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
                if(actionQ.Peek() != Action.backflip)
                {
                    //flip on inflection point
                    if (transform.position.x > path.Peek().x && facingRight)
                    {
                        facingRight = false;
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);

                    }
                    else if (transform.position.x < path.Peek().x && !facingRight)
                    {
                        facingRight = true;
                        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
                    }

                    if (path.Count == 8)
                    {
                        anim.SetBool(landAnim, true);
                    }
                }
                
                //Debug.DrawLine(transform.position, path.Peek(), Color.magenta, 5f);
                transform.position = path.Dequeue();
                if (!reachedEndOfPath)
                {
                    return;
                }
            }
        }

        if (isNavigating)
        {
            HorizVelCalc();
        }
     
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
        if (movement.collisions.below) sfxManager.PlayMovement();
    }


    /// <summary>
    /// Claw platform attack
    /// </summary>
    IEnumerator ClawPlatform()
    {
        attacking = true;

        bool clawRight = navTarget.x > transform.position.x;

        if (playerLevelY == jumpPoints[JumpPoint.leftMid].y)
        {
            navTarget.y = jumpPoints[JumpPoint.leftTop].y;
        }
        else if (playerLevelY == jumpPoints[JumpPoint.leftFloor].y)
        {
            navTarget.y = jumpPoints[JumpPoint.leftMid].y;
        }
        else //player on top level
        {
            attacking = false;
            yield break;
        }
        navTarget.x = Mathf.Clamp(playerTransform.position.x,
            jumpPoints[JumpPoint.leftMid].x + 0.05f, jumpPoints[JumpPoint.rightMid].x - 0.05f);

        clawRight = navTarget.x > transform.position.x;

        StartCoroutine(nameof(NavigateTo));
        yield return new WaitForEndOfFrame();

        while (isNavigating)
        {
            if (playerLevelY == jumpPoints[JumpPoint.leftMid].y)
            {
                navTarget.y = jumpPoints[JumpPoint.leftTop].y;
            }
            else if (playerLevelY == jumpPoints[JumpPoint.leftFloor].y)
            {
                navTarget.y = jumpPoints[JumpPoint.leftMid].y;
            }
            else //player on top level
            {
                isNavigating = false;
                StopCoroutine(nameof(NavigateTo));
                attacking = false;
                yield break;
            }
            navTarget.x = Mathf.Clamp(playerTransform.position.x,
                jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);

            clawRight = navTarget.x > transform.position.x;

            yield return null;
        }

        //claw down anim
        sfxManager.PlayCall();
        anim.SetBool(stompAnim, true);
        yield return new WaitForSeconds(0.25f);
        sfxManager.PlayStomp();

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
            yield return new WaitForSeconds(0.2f);
        }

        anim.SetBool(stompAnim, false);

        clawFacingRight.transform.position = new Vector3(-50, -50, clawFacingRight.transform.position.z);
        clawFacingLeft.transform.position = new Vector3(-50, -50, clawFacingLeft.transform.position.z);

        yield return new WaitForSeconds(0.25f);

        actionQ.Dequeue();

        attacking = false;
    }

    /// <summary>
    /// Backflip move
    /// </summary>
    IEnumerator Backflip()
    {
        attacking = true;

        anim.SetTrigger(backflipAnim);
        yield return new WaitForSeconds(0.25f);

        Bounds platform = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, Mathf.Infinity, groundLayer).collider.bounds;

        //generate path
        GeneratePath(new Vector2(Mathf.Clamp(transform.position.x + (facingRight ? -8 : 8),
            platform.min.x + 2, platform.max.x - 2), transform.position.y), 0.25f);

        sfxManager.PlayJump();
        yield return new WaitForEndOfFrame();
        yield return new WaitUntil(() => reachedEndOfPath);
        
        if(phase == Phase.two)
        {
            sfxManager.PlayCall();
            yield return new WaitForSeconds(0.15f);
            //claw down anim
            anim.SetTrigger(stompAnim);
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);
            sfxManager.PlayStomp();

            int directionMod = facingRight ? 1 : -1;
            for (int i = 0; i < 4; i++)
            {
                if (platform.min.x < transform.position.x + (i * directionMod) || transform.position.x + (i * directionMod) < platform.max.x)
                {
                    clawUp.transform.position = new Vector3(transform.position.x + ((3 + i) * directionMod),
                        platform.max.y, clawUp.transform.position.z);
                    clawUp.transform.localScale = new Vector3(0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f, 0.5f + (1f / (i + 1)) * 0.5f);
                }
                else
                {
                    clawUp.transform.position = new Vector3(-50, -50, clawUp.transform.position.z);
                }

                yield return new WaitForSeconds(0.2f);
            }

            anim.SetBool(stompAnim, false);

            clawUp.transform.position = new Vector3(-50, -50, clawUp.transform.position.z);

            yield return new WaitForSeconds(0.25f);

            actionQ.Dequeue();
        }
        else
        {
            yield return new WaitForSeconds(0.25f);

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

        speedMod = 1f;

        if (transform.position.x > playerTransform.position.x)
        {
            navTarget.y = playerLevelY;
            navTarget.x = Mathf.Clamp(playerTransform.position.x + 5f,
                jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
        }
        else
        {
            navTarget.y = playerLevelY;
            navTarget.x = Mathf.Clamp(playerTransform.position.x - 5f,
                jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
        }

        StartCoroutine(nameof(NavigateTo));
        yield return new WaitForEndOfFrame();

        while (isNavigating) {
            if (transform.position.x > playerTransform.position.x)
            {
                navTarget.y = playerLevelY;
                navTarget.x = Mathf.Clamp(playerTransform.position.x + 5f,
                    jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
            }
            else
            {
                navTarget.y = playerLevelY;
                navTarget.x = Mathf.Clamp(playerTransform.position.x - 5f,
                    jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
            }
            yield return null;
        }
        

        if(playerTransform.position.x < transform.position.x)
        {
            facingRight = false;
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }
        else
        {
            facingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        }

        anim.SetBool(noTailAnim, true);

        switch (phase)
        {
            case Phase.one:
                anim.SetTrigger(tailAnim);
                yield return new WaitForSeconds(27f / 60f);
                TailSwipeScript ts = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (!facingRight)
                {
                    ts.direction = -1;
                }
                sfxManager.PlayTailWhip();
                break;
            case Phase.two:
                anim.SetTrigger(doubleTailAnim);
                yield return new WaitForSeconds(27f / 60f);
                TailSwipeScript ts1 = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (!facingRight)
                {
                    ts1.direction = -1;
                }
                sfxManager.PlayTailWhip();
                yield return new WaitForSeconds(1f / 3f);
                TailSwipeScript ts2 = Instantiate(tailSwipePrefab, tailEnd.position, Quaternion.identity).GetComponent<TailSwipeScript>();
                if (!facingRight)
                {
                    ts2.direction = -1;
                }
                sfxManager.PlayTailWhip();
                break;
        }
        
        //wait for end of anim
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        anim.SetBool(noTailAnim, false);

        yield return new WaitForSeconds(0.25f);

        actionQ.Dequeue();

        attacking = false;
    }

    /// <summary>
    /// Claw swipe attack
    /// </summary>
    IEnumerator ClawSwipe()
    {
        attacking = true;

        speedMod = 1f;

        if (transform.position.x > playerTransform.position.x)
        {
            navTarget.y = playerLevelY;
            navTarget.x = Mathf.Clamp(playerTransform.position.x + 5.25f,
                jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
        }
        else
        {
            navTarget.y = playerLevelY;
            navTarget.x = Mathf.Clamp(playerTransform.position.x - 5.25f,
                jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
        }

        StartCoroutine(nameof(NavigateTo));
        yield return new WaitForEndOfFrame();

        while (isNavigating)
        {
            if (transform.position.x > playerTransform.position.x)
            {
                navTarget.y = playerLevelY;
                navTarget.x = Mathf.Clamp(playerTransform.position.x + 5.25f,
                    jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
            }
            else
            {
                navTarget.y = playerLevelY;
                navTarget.x = Mathf.Clamp(playerTransform.position.x - 5.25f,
                    jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
            }
            yield return null;
        }

        switch (phase)
        {
            case Phase.one:
                anim.SetTrigger(clawAnim);
                sfxManager.PlaySwipe();

                //wait until(Anim is done)
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

                actionQ.Dequeue();

                break;
            case Phase.two:
                anim.SetTrigger(eyeFlashAnim);
                sfxManager.PlayEyeFlash();

                //wait until first anim is done
                yield return new WaitForEndOfFrame();
                yield return new WaitForSeconds(17f/60f);

                int animBool;
                //move quickly towards player
                speedMod = 6f;
                if(transform.position.x < playerTransform.position.x)
                {
                    moveTarget.x = Mathf.Clamp(playerTransform.position.x - 0.75f,
                        jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
                    animBool = blurRightAnim;
                }
                else
                {
                    moveTarget.x = Mathf.Clamp(playerTransform.position.x + 0.75f,
                        jumpPoints[JumpPoint.leftMid].x, jumpPoints[JumpPoint.rightMid].x);
                    animBool = blurLeftAnim;
                }

                anim.SetBool(animBool, true);

                isNavigating = true;
                yield return new WaitForFixedUpdate();
                yield return new WaitWhile(() => IsMoving);
                isNavigating = false;

                anim.SetBool(animBool, false);

                anim.SetTrigger(clawAnim);
                sfxManager.PlaySwipe();
                //wait until claw anim is done
                yield return new WaitForFixedUpdate();
                yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length - anim.GetCurrentAnimatorStateInfo(0).normalizedTime);

                actionQ.Dequeue();

                break;
        }

        yield return new WaitForSeconds(0.25f);

        attacking = false;
    }

    /// <summary>
    /// Charge attack
    /// </summary>
    IEnumerator Charge()
    {
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

        StartCoroutine(nameof(NavigateTo));
        yield return new WaitForEndOfFrame();

        while (isNavigating)
        {
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
            yield return null;
        }

        int animBool = (playerTransform.position.x > transform.position.x) ? blurRightAnim : blurLeftAnim;
        anim.SetBool(animBool, true);
        sfxManager.PlayCall();

        //store start point
        Vector2 start = transform.position;

        //set target and speed
        RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up, Vector2.down, 3f, groundLayer);
        if (hit)
        {
            if (playerTransform.position.x < transform.position.x)
            {
                moveTarget = new Vector2(Mathf.Max(hit.collider.bounds.min.x + 2, playerTransform.position.x - 4), start.y);
            }
            else
            {
                moveTarget = new Vector2(Mathf.Min(hit.collider.bounds.max.x - 2, playerTransform.position.x + 4), start.y);
            }

        }
        speedMod = 4f;

        isNavigating = true;
        yield return new WaitForFixedUpdate();
        yield return new WaitWhile(() => IsMoving);
        isNavigating = false;

        anim.SetBool(animBool, false);

        if(phase == Phase.two)//go back
        {
            yield return new WaitForSeconds(0.2f);

            int animBoolP2 = (playerTransform.position.x > transform.position.x) ? blurRightAnim : blurLeftAnim;
            anim.SetBool(animBoolP2, true);
            sfxManager.PlayCall();

            //go back
            moveTarget = start;

            isNavigating = true;
            yield return new WaitForFixedUpdate();
            yield return new WaitWhile(() => IsMoving);
            isNavigating = false;

            anim.SetBool(animBoolP2, false);
        }

        yield return new WaitForSeconds(0.25f);

        actionQ.Dequeue();

        attacking = false;
    }


    IEnumerator NavigateTo()
    {
        isNavigating = true;

        JumpPoint jumpStart;
        JumpPoint jumpEnd;
        bool leftOfCenter = transform.position.x < centerX;

        //same level
        if (Mathf.Abs(navTarget.y - transform.position.y) < 1.5f)
        {
            
            speedMod = 1f;
            moveTarget = navTarget;
        }
        else //move to a jump point
        {
            isMoving = true;
            do
            {
                //determine closest jump point
                if (leftOfCenter) //left side
                {
                    if (Mathf.Abs(transform.position.y - jumpPoints[JumpPoint.leftFloor].y) < 1)//on floor
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
                yield return null;
            }
            while (isMoving);

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

            if (jumpPoints[jumpEnd].y > transform.position.y)
            {
                anim.SetTrigger(upJumpAnim);
            }
            else
            {
                anim.SetTrigger(downJumpAnim);
            }
            anim.SetBool(landAnim, false);

            //jump anim start
            yield return new WaitForSeconds(0.25f);
            sfxManager.PlayJump();

            if (jumpStart != jumpEnd)
            {
                if (jumpStart == JumpPoint.leftFloor || jumpStart == JumpPoint.rightFloor)
                {
                    GeneratePath(transform.position, jumpPoints[jumpEnd], 0.5f);
                }
                else
                {
                    if (leftOfCenter)
                    {
                        GeneratePath(transform.position, jumpPoints[JumpPoint.leftWall], 0.5f);
                        GeneratePath(jumpPoints[JumpPoint.leftWall], jumpPoints[jumpEnd], 0.5f);
                    }
                    else
                    {
                        GeneratePath(transform.position, jumpPoints[JumpPoint.rightWall], 0.5f);
                        GeneratePath(jumpPoints[JumpPoint.rightWall], jumpPoints[jumpEnd], 0.5f);
                    }
                }
            }
        }

        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => reachedEndOfPath);

        moveTarget.x = navTarget.x;
        moveTarget.y = transform.position.y;
        yield return new WaitForFixedUpdate();
        do
        {
            moveTarget.x = navTarget.x;
            moveTarget.y = transform.position.y;
            yield return null;
        } while (isMoving);

        isNavigating = false;
    }

    /// <summary>
    /// Checks x distance for being close enough to snap to target
    /// </summary>
    /// <returns>True if snapped to target x</returns>
    bool CheckXDistToMoveTarget()
    {
        if(Mathf.Abs(moveTarget.y - transform.position.y) < 3f)
        {
            if (Mathf.Abs(moveTarget.x - transform.position.x) <  2 * speed * Time.fixedDeltaTime * speedMod)
            {
                transform.position = new Vector3(moveTarget.x, transform.position.y, transform.position.z);
                velocity.x = 0;
                IsMoving = false;
                sfxManager.StopMovement();
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
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, 1);

        }
        else if(velocity.x > 0 && !facingRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
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
        int randomIndex = Random.Range(0, 6 + (clawable ? 2 : 0));
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
    void GeneratePath(Vector2 endPoint, float timeToMove)
    {
        GeneratePath(transform.position, endPoint, timeToMove);
    }

    /// <summary>
    /// Generates a path from start to end point
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    void GeneratePath(Vector2 startPoint, Vector2 endPoint, float timeToMove)
    {
        endPoint.y += 0.1f;
        //Debug.DrawLine(startPoint, endPoint, Color.red, 5f);

        reachedEndOfPath = false;
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
