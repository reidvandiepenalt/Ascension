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

    [SerializeField] Transform mouthTransform;
    [SerializeField] GameObject rapidFireballParent;
    [SerializeField] List<RapidFireFireball> rapidFireballs;
    int _rapidFireIndex = 0;
    int RapidFireIndex { get => _rapidFireIndex; 
        set { if (value >= rapidFireballs.Count) { _rapidFireIndex = value - rapidFireballs.Count; }
            else { _rapidFireIndex = value; } } }
    [SerializeField] GameObject fireRainParent;
    [SerializeField] List<FireRainFireball> fireRainFireballs;
    [SerializeField] HomingFireball homingFireball;
    [SerializeField] FirePlume firePlume;
    [SerializeField] FirePlumeFireball diveFireball1, diveFireball2;

    [SerializeField] Vector2 platformTL, platformTR, platformMid, platformBL, platformBR;
    Vector2[] platformPoints;

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

        platformPoints = new Vector2[5] { platformTR, platformTL, platformMid, platformBL, platformBR };

        groundLayer = LayerMask.GetMask("Ground");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;

        availableAttacks = new List<Action>();
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
        //testing
        actionQ.Enqueue(Action.homingBomb);
        phase = Phase.two;

        /*
        float randomFloat = Random.Range(0.0f, 1.0f);

        //1/7 to do landing attack
        if(randomFloat < (1.0f / 7.0f))
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
        }*/
    }

    /// <summary>
    /// Create a lava plume under the player that sets the ground on fire and shoots fireballs in phase 2
    /// </summary>
    IEnumerator Plume()
    {
        isAttacking = true;

        //move to opposite side random quadrant
        Quadrant moveQuad = Quadrant.topLeft;
        switch (PlayerQuadrant())
        {
            case Quadrant.topRight:
            case Quadrant.bottomRight:
                int rand = Random.Range(0, 2);
                if(rand == 0)
                {
                    moveQuad = Quadrant.topLeft;
                }
                else
                {
                    moveQuad = Quadrant.bottomLeft;
                }
                break;
            case Quadrant.topLeft:
            case Quadrant.bottomLeft:
                int rand2 = Random.Range(0, 2);
                if (rand2 == 0)
                {
                    moveQuad = Quadrant.topRight;
                }
                else
                {
                    moveQuad = Quadrant.bottomRight;
                }
                break;
        }
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        //plume anim
        firePlume.Begin(NearestPlatformPoint(playerTransform.position, false), phase);
        //wait for anim to finish

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

        //move to other quad on player side
        Quadrant moveQuad = Quadrant.topLeft;
        switch (PlayerQuadrant())
        {
            case Quadrant.topRight:
                moveQuad = Quadrant.bottomRight;
                break;
            case Quadrant.topLeft:
                moveQuad = Quadrant.bottomLeft;
                break;
            case Quadrant.bottomLeft:
                moveQuad = Quadrant.topLeft;
                break;
            case Quadrant.bottomRight:
                moveQuad = Quadrant.topRight;
                break;
        }
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        //homing fireball anim
        homingFireball.Begin(playerTransform, transform.position + Vector3.up * centerOffset, phase);
        //wait for anim to finish

        //testing
        yield return new WaitForSeconds(2.5f);

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

        //move to player quad
        moveTarget = RandomQuadrantPosition(PlayerQuadrant());
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        //fire arc anim
        bool p1 = phase == Phase.one;
        float centerAngle = Mathf.Atan2(playerTransform.position.y - mouthTransform.position.y, playerTransform.position.x - mouthTransform.position.x);
        int offset = p1 ? 3 : 5;
        for (int i = -offset; i <= offset; i++)
        {
            rapidFireballs[RapidFireIndex].Begin(mouthTransform.position, centerAngle + (i * (5f * Mathf.Deg2Rad)), p1 ? 14 : 18);
            RapidFireIndex++;
            yield return new WaitForSeconds(0.025f);
        }
        //wait for anim to finish

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

        //move to opposite quad
        Quadrant moveQuad = Quadrant.topLeft;
        switch (PlayerQuadrant())
        {
            case Quadrant.topRight:
                moveQuad = Quadrant.bottomLeft;
                break;
            case Quadrant.topLeft:
                moveQuad = Quadrant.bottomRight;
                break;
            case Quadrant.bottomLeft:
                moveQuad = Quadrant.topRight;
                break;
            case Quadrant.bottomRight:
                moveQuad = Quadrant.topLeft;
                break;
        }
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        //fire beam anim
        float angle = Mathf.Atan2(playerTransform.position.y - mouthTransform.position.y, playerTransform.position.x - mouthTransform.position.x);
        bool p1 = phase == Phase.one;
        float time = 0.0f;
        while(time < (p1 ? 0.6f : 0.75f))
        {
            //face player
            if (playerTransform.position.x > transform.position.x && !facingRight)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingRight = true;
            }
            else if (playerTransform.position.x < transform.position.x && facingRight)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                facingRight = false;
            }

            float newAngle = Mathf.Atan2(playerTransform.position.y - mouthTransform.position.y, playerTransform.position.x - mouthTransform.position.x);
            angle = Mathf.LerpAngle(angle, newAngle, p1 ? 2f : 10f);
            rapidFireballs[RapidFireIndex].Begin(mouthTransform.position, angle, p1 ? 24 : 28);
            RapidFireIndex++;
            time += Time.deltaTime;
            yield return new WaitForSeconds(0.05f);
        }
        //wait for anim to finish

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

        //move to random quad
        Quadrant moveQuad = (Quadrant) Random.Range(0, 4);
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        //fire rain anim
        for(int i = 0; i < fireRainFireballs.Count / (phase == Phase.one ? 2 : 1); i++)
        {
            fireRainFireballs[i].Begin(new Vector2(Random.Range(arenaMinX, arenaMaxX), 50));

            yield return new WaitForSeconds(0.1f);
        }
        //wait for anim to finish

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

        //move to top of arena
        isMoving = true;
        moveTarget = new Vector2(transform.position.x, arenaMaxY);
        yield return new WaitWhile(() => isMoving);

        //dive to player closest platform
        //dive anim
        speedMod = 2;

        isMoving = true;
        moveTarget = NearestPlatformPoint(playerTransform.position, true) + (Vector2.up * 0.5f);
        yield return new WaitWhile(() => isMoving);
        speedMod = 1;

        if(phase == Phase.two)
        {
            //shoot fireballs
            diveFireball1.Launch(transform.position, 45);
            diveFireball2.Launch(transform.position, 135);
        }

        //wait for anim to finish

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

        //face player
        if (playerTransform.position.x > transform.position.x && !facingRight)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = true;
        }
        else if (playerTransform.position.x < transform.position.x && facingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            facingRight = false;
        }

        //start beam anim
        bool p1 = phase == Phase.one;
        float angle = facingRight ? 0 : 180;
        float time = 0.0f;
        while (time < 0.6f)
        {
            rapidFireballs[RapidFireIndex].Begin(mouthTransform.position, angle * Mathf.Deg2Rad);
            RapidFireIndex++;
            time += Time.deltaTime;
            yield return new WaitForSeconds(0.05f);
        }
        if (!p1)
        {
            //start head move anim
            while (time < 1f)
            {
                angle = Mathf.LerpAngle(angle, 90, 0.15f);
                rapidFireballs[RapidFireIndex].Begin(mouthTransform.position, angle * Mathf.Deg2Rad);
                RapidFireIndex++;
                time += Time.deltaTime;
                yield return new WaitForSeconds(0.05f);
            }
        }
        //wait for anim to finish

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

        speedMod = 0.6f;
        Quadrant moveQuad = Quadrant.topRight;
        switch (PlayerQuadrant())
        {
            case Quadrant.topRight:
            case Quadrant.bottomRight:
                moveQuad = Quadrant.topLeft;
                break;
            case Quadrant.topLeft:
            case Quadrant.bottomLeft:
                moveQuad = Quadrant.topRight;
                break;
        }
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        if(moveQuad == Quadrant.topLeft)
        {
            moveQuad = Quadrant.topRight;
        }
        else
        {
            moveQuad = Quadrant.topLeft;
        }
        moveTarget = RandomQuadrantPosition(moveQuad);
        isMoving = true;
        yield return new WaitWhile(() => isMoving);

        speedMod = 1f;
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

    /// <summary>
    /// Helper function for finding the nearest platform / ground point to the given position
    /// </summary>
    /// <param name="pos">Position to find nearest platform to</param>
    /// <param name="platformOnly">Ignore ground floor?</param>
    /// <returns>Nearest platform point</returns>
    Vector2 NearestPlatformPoint(Vector2 pos, bool platformOnly)
    {
        Vector2 curPoint = new Vector2(pos.x, arenaMinY);
        float minDist = pos.y;
        if (platformOnly)
        {
            curPoint = platformPoints[0];
            minDist = Vector2.Distance(pos, platformPoints[0]);
        }

        for(int i = (platformOnly)?1:0; i < platformPoints.Length; i++)
        {
            float platDist = Vector2.Distance(pos, platformPoints[i]);
            if (platDist <= minDist)
            {
                minDist = platDist;
                curPoint = platformPoints[i];
            }
        }
        return curPoint;
    }
}
