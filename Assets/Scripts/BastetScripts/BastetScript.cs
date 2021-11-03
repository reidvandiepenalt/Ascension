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

    [SerializeField] GameObject swipePrefab, clawPrefab;

    [SerializeField] float speed;
    bool facingRight = true;
    float speedMod = 1f;
    Vector2 velocity;
    Vector2 moveTarget;

    Queue<Vector2> path = new Queue<Vector2>();
    bool reachedEndOfPath = true;

    float gravity = -80;//same as player

    [SerializeField] Transform playerTransform;
    Collider2D playerCollider;
    float playerGroundOffset;

    bool isMoving = false;
    Queue<Action> actionQ = new Queue<Action>();
    Phase phase = Phase.one;
    bool actionSetupIncomplete = true;

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

        switch (actionQ.Peek())
        {
            case Action.walk:
                speedMod = 0.5f;
                if (CheckXDistToMoveTarget()) { actionQ.Dequeue(); }
                break;
            case Action.sprint:
                speedMod = 1f;
                if (CheckXDistToMoveTarget()) { actionQ.Dequeue(); }
                break;
            case Action.charge:
                Charge();
                break;
            case Action.clawSwipe:
                break;
            case Action.tailWhip:
                break;
            case Action.clawPlatform:
                break;
            case Action.backflip:
                break;
        }

        movement.Move(Time.fixedDeltaTime * velocity);
    }


    /// <summary>
    /// Claw platform attack
    /// </summary>
    void ClawPlatform()
    {
        switch (phase)
        {
            case Phase.one:
                //move to nearest jump point and jump up, then go above player and attack
                break;
            case Phase.two:

                break;
        }
    }

    /// <summary>
    /// Backflip move
    /// </summary>
    void Backflip()
    {
        switch (phase)
        {
            case Phase.one:
                if (actionSetupIncomplete)
                {
                    actionSetupIncomplete = false;

                    //start anim

                    //generate path
                    GeneratePath(new Vector2(transform.position.x + (facingRight ? -8 : 8), transform.position.y));
                }else if (reachedEndOfPath)
                {
                    //stop anim?

                    actionSetupIncomplete = true;

                    actionQ.Dequeue();
                }
                break;
            case Phase.two:
                break;
        }
    }

    /// <summary>
    /// Tail swipe attack
    /// </summary>
    void TailSwipe()
    {
        switch (phase)
        {
            case Phase.one:
                if (actionSetupIncomplete)
                {
                    actionSetupIncomplete = false;

                    //start anim
                }
                else //if(anim is done)
                {
                    actionSetupIncomplete = true;
                    actionQ.Dequeue();
                }
                break;
            case Phase.two:
                break;
        }
    }

    /// <summary>
    /// Claw swipe attack
    /// </summary>
    void ClawSwipe()
    {
        switch (phase)
        {
            case Phase.one:
                if (actionSetupIncomplete)
                {
                    actionSetupIncomplete = false;

                    //start anim
                }
                else //if(Anim is done)
                {
                    actionSetupIncomplete = true;
                    actionQ.Dequeue();
                }
                break;
            case Phase.two:
                break;
        }
    }

    /// <summary>
    /// Charge attack
    /// </summary>
    void Charge()
    {
        switch (phase)
        {
            case Phase.one:
                if (actionSetupIncomplete)
                {
                    actionSetupIncomplete = false;

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
                }
                else if (!isMoving)
                {
                    //stop anim

                    actionQ.Dequeue();
                    actionSetupIncomplete = true;
                }
                break;
            case Phase.two:

                break;
        }
    }

    /// <summary>
    /// Checks x distance for being close enough to snap to target
    /// </summary>
    /// <returns>True if snapped to target x</returns>
    bool CheckXDistToMoveTarget()
    {
        if (moveTarget.x - transform.position.x < speed * Time.fixedDeltaTime * speedMod * velocity.x)
        {
            transform.position = new Vector3(moveTarget.x, transform.position.y, transform.position.z);
            velocity.x = 0;
            isMoving = false;
            return true;
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
        reachedEndOfPath = false;
        Vector2 vertex = transform.position;
        float timeToMove = 0.3f;
        int numSteps = (int)(60f * timeToMove);
        float stepDist = (endPoint.x - vertex.x) / numSteps;

        float initVy = (endPoint.y - transform.position.y - gravity / 2 * Mathf.Pow(timeToMove, 2)) / timeToMove;
        for (int i = 0; i < numSteps; i++)
        {
            path.Enqueue(new Vector2(vertex.x + i * stepDist,
                gravity / 2 * Mathf.Pow(i * Time.fixedDeltaTime, 2) + (i * Time.fixedDeltaTime * initVy) + vertex.y));
        }
    }
}
