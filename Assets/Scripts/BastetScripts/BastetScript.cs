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
    Vector2 jumpTarget;

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
                jumpPoints.Add((JumpPoint)i, ts[i].position);
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

        movement.Move(speed * Time.fixedDeltaTime * speedMod * velocity.normalized);
    }



    bool CheckXDistToMoveTarget()
    {
        if (moveTarget.x - transform.position.x < speed * Time.fixedDeltaTime * speedMod * velocity.normalized.x)
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
}
