using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetScript : MonoBehaviour
{
    [SerializeField] Dictionary<JumpPoint, Vector2> jumpPoints;
    [SerializeField] GameObject jumpParent;
    [SerializeField] EnemyCollisionMovementHandler movement;
    [SerializeField] EnemyHealth healthManager;

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

    Queue<Action> actionQ = new Queue<Action>();
    Phase phase = Phase.one;

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

    List<Action> attackOrder = new List<Action>() { Action.clawSwipe, Action.backflip, Action.charge, Action.tailWhip, Action.clawPlatform };

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
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;

        phase = Phase.one;

        //testing
        transform.position = jumpPoints[JumpPoint.rightFloor];
        GeneratePath(jumpPoints[JumpPoint.rightMid]);
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
        }

        movement.Move(speed * Time.fixedDeltaTime * speedMod * velocity);
    }



    bool CheckXDistToMoveTarget()
    {
        if (moveTarget.x - transform.position.x < speed * Time.fixedDeltaTime * speedMod * velocity.x)
        {
            transform.position = new Vector3(moveTarget.x, transform.position.y, transform.position.z);
            velocity.x = 0;
            return true;
        }
        return false;
    }

    void HorizVelCalc()
    {
        velocity.x = moveTarget.x - transform.position.x;
        if(velocity.x < 0 && facingRight)
        {
            facingRight = false;

        }else if(velocity.x > 0 && !facingRight)
        {
            facingRight = true;
            //flip gfx
        }
    }

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
