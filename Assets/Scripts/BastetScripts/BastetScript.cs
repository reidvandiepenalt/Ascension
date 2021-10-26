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

    State state = State.walk;
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
    enum State
    {
        walk,
        sprint,
        charge,
        clawSwipe,
        tailWhip,
        clawPlatform,
        backflip
    }

    List<State> attackOrder = new List<State>() { State.clawSwipe, State.backflip, State.charge, State.tailWhip, State.clawPlatform };
    State CurrentAttack
    {
        get => state; set
        {
            attackOrder.Add(state);
            state = value;
            attackOrder.Remove(value);
        }
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


        switch (state)
        {
            case State.walk:
                speedMod = 0.5f;
                if (CheckXDistToMoveTarget()) { PickAttack(); }
                break;
            case State.sprint:
                speedMod = 1f;
                if (CheckXDistToMoveTarget()) { PickAttack(); }
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
        State[] pickableAtks = new State[attackOrder.Count - 1];

        //only do claw platform if player not on top
        if(playerTransform.position.y + playerGroundOffset < jumpPoints[JumpPoint.leftTop].y)
        {
            int clawIndex = attackOrder.IndexOf(State.clawPlatform);
            for(int i = 0; i < attackOrder.Count; i++)
            {
                if(i > clawIndex)
                {
                    pickableAtks[i - 1] = attackOrder[i];
                }else if (i < clawIndex)
                {
                    pickableAtks[i] = attackOrder[i];
                }
            }
        }

        //random new attack; weighted towards using attacks it hasnt used in a while
        int randomWeight = Random.Range(0, 1);
        if (randomWeight < 0.45f)
        {
            CurrentAttack = pickableAtks[0];
        }
        else if (randomWeight < 0.75f)
        {
            CurrentAttack = pickableAtks[1];
        }
        else if (randomWeight < 0.95f)
        {
            CurrentAttack = pickableAtks[2];
        }
        else
        {
            CurrentAttack = pickableAtks[3];
        }

        //start new attack
        switch (state)
        {
            case State.charge:
                break;
            case State.clawSwipe:
                break;
            case State.tailWhip:
                break;
            case State.clawPlatform:
                break;
            case State.backflip:
                break;
        }
    }
}
