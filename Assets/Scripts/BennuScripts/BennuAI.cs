using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BennuAI : MonoBehaviour
{
    [SerializeField] EnemyHealth healthManager;
    [SerializeField] Animator anim;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float arenaMaxX, arenaMinX, arenaMaxY, arenaMinY;
    float arenaCenterX, arenaCenterY;

    [SerializeField] float speed;
    float speedMod = 1;
    bool facingRight = false;
    float gravity = -80;//same as player
    const float centerOffset = 3.82f;
    Vector2 moveTarget;

    Action attack;
    Phase phase = Phase.one;

    [SerializeField] GameObject rapidFireballParent;
    List<RapidFireFireball> rapidFireballs;
    [SerializeField] GameObject fireRainParent;
    List<FireRainFireball> fireRainFireballs;
    [SerializeField] HomingFireball homingFireball;
    

    [SerializeField] Transform playerTransform;
    Collider2D playerCollider;
    float playerGroundOffset;

    bool isMoving = false;
    bool isAttacking = false;
    Queue<Action> actionQ = new Queue<Action>();
    readonly List<Action> pickableAttacks = new List<Action> { Action.plume, Action.homingBomb, Action.fireArc, Action.fireBeam, Action.fireRain };
    List<Action> availableAttacks;

    enum Action {
        plume,
        homingBomb,
        fireArc,
        fireBeam,
        fireRain,
        diveLand,
        landedBeam,
        recoveryMovement
    }

    public enum Phase
    {
        one,
        two,
        three
    }

    enum Quadrant
    {
        topRight,
        topLeft,
        bottomLeft,
        bottomRight
    }

    public void OnStun(object param)
    {
        float time = (float)param;
        //stun for a given time

    }

    public void OnHit(object parameter)
    {
        int health = (int)parameter;
        if (health < 0 && phase == Phase.three)
        {
            Die();
        }
        else if (health < (healthManager.MaxHealth / 2f) && phase == Phase.one)
        {
            phase = Phase.two;
        }
        else if (health < 15)
        {
            if(phase == Phase.two)
            {
                healthManager.SetHealth(15);
                phase = Phase.three;
            }
        }
    }

    void Die()
    {

    }

    private void OnValidate()
    {
        rapidFireballs = new List<RapidFireFireball>();
        rapidFireballs.AddRange(rapidFireballParent.GetComponentsInChildren<RapidFireFireball>());
        fireRainFireballs = new List<FireRainFireball>();
        fireRainFireballs.AddRange(fireRainParent.GetComponentsInChildren<FireRainFireball>());
    }

    // Start is called before the first frame update
    void Start()
    {
        arenaCenterX = (arenaMaxX - arenaMinX) / 2 + arenaMinX;
        arenaCenterY = (arenaMaxY - arenaMinY) / 2 + arenaMinY;

        groundLayer = LayerMask.GetMask("Ground");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        if(actionQ.Count == 0) { PickAttack(); }

        if (!isAttacking)
        {
            switch (actionQ.Peek())
            {
                case Action.plume:
                    StartCoroutine(nameof(Plume));
                    break;
                case Action.homingBomb:
                    StartCoroutine(nameof(HomingBomb));
                    break;
                case Action.fireArc:
                    StartCoroutine(nameof(FireArc));
                    break;
                case Action.fireBeam:
                    StartCoroutine(nameof(FireBeam));
                    break;
                case Action.fireRain:
                    StartCoroutine(nameof(FireRain));
                    break;
                case Action.diveLand:
                    StartCoroutine(nameof(DiveLand));
                    break;
                case Action.landedBeam:
                    StartCoroutine(nameof(LandedBeam));
                    break;
                case Action.recoveryMovement:
                    StartCoroutine(nameof(RecoveryMovement));
                    break;
            }
        }

        if (isMoving) { MoveStraight(); }
    }


    /// <summary>
    /// Moves toward the current moveTarget if isMoving at speed
    /// </summary>
    void MoveStraight()
    {
        //reached destination
        if (Vector2.Distance(transform.position, moveTarget) < speed * Time.deltaTime)
        {
            isMoving = false;
            transform.position = moveTarget;
        }
        //move toward destination
        else
        {
            Vector2 movement = (moveTarget - (Vector2)transform.position).normalized * speed * Time.deltaTime;
            transform.position += new Vector3(movement.x, movement.y, 0);
      
            if (movement.x > 0 && !facingRight)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingRight = true;
            }
            else if (movement.x < 0 && facingRight)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingRight = false;
            }
        }
    }

    /// <summary>
    /// Pick attack for Bennu
    /// </summary>
    void PickAttack()
    {
        float randomFloat = Random.Range(0.0f, 1.0f);

        //1/7 to do landing attack
        if(randomFloat < 1.0f / 7.0f)
        {
            actionQ.Enqueue(Action.diveLand);
            actionQ.Enqueue(Action.landedBeam);
            actionQ.Enqueue(Action.recoveryMovement);
        }
        else
        {
            availableAttacks.Clear();
            availableAttacks.AddRange(pickableAttacks);
            for(int i = 0; i < 2; i++) //add two random attacks to action Q (might change to 3)
            {
                int index = Random.Range(0, availableAttacks.Count);
                actionQ.Enqueue(availableAttacks[index]);
                availableAttacks.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Create a lava plume under the player that sets the ground on fire and shoots fireballs in phase 2
    /// </summary>
    IEnumerator Plume()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Spawn a large, slowmoving, homing fireball that breaks into smaller fireballs after a certain amount of time. p2 moves faster and splits into more fireballs
    /// </summary>
    IEnumerator HomingBomb()
    {
        isAttacking = true;



        homingFireball.Begin(playerTransform, transform.position + Vector3.up * centerOffset, (int)phase + 1);

        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Create a wide arc of fire that moves in the direction of the player. p2 is wider and faster
    /// </summary>
    IEnumerator FireArc()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Shoot a beam of fire towards the player and follow their movement. p2 is longer and faster turning
    /// </summary>
    IEnumerator FireBeam()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Rain fire down from the ceiling that sets the floor on fire when landing. p2 has more fire
    /// </summary>
    IEnumerator FireRain()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Dive towards the platform that the player is on, attacking while diving. p2 shoots fireballs from landing point
    /// </summary>
    IEnumerator DiveLand()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Shoot a horizontal beam while landed. in p2, turn upwards to the ceiling
    /// </summary>
    IEnumerator LandedBeam()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// After a set of attacks, move slower to a far position to allow player to get some free damage
    /// </summary>
    IEnumerator RecoveryMovement()
    {
        isAttacking = true;


        actionQ.Dequeue();
        isAttacking = false;
        yield return null;
    }

    /// <summary>
    /// Helper function for getting a random position in a given quadrant
    /// </summary>
    /// <param name="quadrant">cartesian quadrants</param>
    /// <returns>random position in the quadrant</returns>
    Vector2 RandomQuadrantPosition(Quadrant quad)
    {
        switch (quad)
        {
            case Quadrant.topRight:
                return new Vector2(Random.Range(arenaCenterX, arenaMaxX), Random.Range(arenaCenterY, arenaMaxY));
            case Quadrant.topLeft:
                return new Vector2(Random.Range(arenaMinX, arenaCenterX), Random.Range(arenaCenterY, arenaMaxY));
            case Quadrant.bottomLeft:
                return new Vector2(Random.Range(arenaMinX, arenaCenterX), Random.Range(arenaMinY, arenaCenterY));
            case Quadrant.bottomRight:
                return new Vector2(Random.Range(arenaCenterX, arenaMaxX), Random.Range(arenaMinY, arenaCenterY));
            default:
                return Vector2.zero;
        }
    }

    /// <summary>
    /// Helper function for determining what quadrant the player is in
    /// </summary>
    /// <returns></returns>
    Quadrant PlayerQuadrant()
    {
        if(playerTransform.position.x > arenaCenterX) //right side
        {
            if(playerTransform.position.y > arenaCenterY)//top right
            {
                return Quadrant.topRight;
            }
            else //bottom right
            {
                return Quadrant.bottomRight;
            }
        }
        else //left side
        {
            if (playerTransform.position.y > arenaCenterY)//top left
            {
                return Quadrant.topLeft;
            }
            else //bottom left
            {
                return Quadrant.bottomLeft;
            }
        }
    }
}
