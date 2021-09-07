using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusAI : MonoBehaviour, IEnemy
{
    enum Attack
    {
        idle,
        dive,
        rain,
        shotgun,
        gusts,
        //peck,?
        xAttack,
        swoop
    }
    Attack curAttack = Attack.idle;
    Attack prevAttack = Attack.idle;
    Attack CurrentAttack { get => curAttack;  set { prevAttack = curAttack; curAttack = value; } }

    enum Phase
    {
        one,
        two,
        three
    }
    Phase phase = Phase.one;


    public Vector2 topLeft;
    public Vector2 bottomRight;
    public LayerMask groundMask;

    public HorusGustScript gustInst;
    public List<HorusFeatherScript> disabledFeathers;
    public List<HorusFeatherScript> enabledFeathers;

    public Animator anim;

    Vector2 moveTarget = Vector2.zero;
    bool isMoving = false;

    public float rainShotDelay;
    public int shotgunCount;
    public int shotgunConeDeg;

    public float speed;
    float speedMod;

    public Transform playerTransform;
    float playerGroundOffset;

    System.Random rng;

    public int Health { get; set; }
    public int MaxHealth { get; set; }

    public void Stun(float time)
    {
        throw new System.NotImplementedException();
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if(Health < 0)
        {
            Die();
        }else if(Health < (MaxHealth / 3f))
        {
            phase = Phase.three;
        }else if (Health < (2 * MaxHealth / 3f))
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
        Health = MaxHealth;
        rng = new System.Random();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerGroundOffset = playerTransform.gameObject.GetComponent<Collider2D>().bounds.extents.y;
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        //Entry point
        if(CurrentAttack == Attack.idle)
        {
            PickAttack();
        }

        if (isMoving)
        {
            MoveStraight();
        }
    }

    /// <summary>
    /// Moves toward the current moveTarget if isMoving at speed * speedmod
    /// </summary>
    void MoveStraight()
    {
        if (isMoving)
        {
            //reached destination
            if (Vector2.Distance(transform.position, moveTarget) < speed * speedMod * Time.deltaTime)
            {
                isMoving = false;
                transform.position = moveTarget;
            }
            //move toward destination
            else
            {
                Vector2 movement = (moveTarget - (Vector2)transform.position).normalized * speed * speedMod * Time.deltaTime;
                transform.position += new Vector3(movement.x, movement.y, 0);
            }
        }
    }

    /// <summary>
    /// Picks a new attack for horus
    /// </summary>
    void PickAttack()
    {
        //random new attack; make smarter?
        /*List<Attack> possibleAttacks = new List<Attack>() { Attack.dive, Attack.gusts, Attack.rain, Attack.shotgun, Attack.wing, Attack.xAttack, Attack.swoop };
        possibleAttacks.Remove(prevAttack);
        CurrentAttack = possibleAttacks[rng.Next(0, possibleAttacks.Count - 1)];*/
        CurrentAttack = Attack.xAttack;

        //start new attack
        switch (CurrentAttack)
        {
            case Attack.idle:
                break;
            case Attack.dive:
                StartCoroutine(Dive());
                break;
            case Attack.gusts:
                StartCoroutine(Gust());
                break;
            case Attack.rain:
                StartCoroutine(Rain());
                break;
            case Attack.shotgun:
                StartCoroutine(Shotgun());
                break;
            case Attack.xAttack:
                StartCoroutine(XAttack());
                break;
            case Attack.swoop:
                StartCoroutine(Swoop());
                break;
        }
    }

    IEnumerator Dive()
    {
        switch (phase)
        {
            case Phase.one:
                //pick location (above player, keep updating)
                isMoving = true;
                speedMod = 1;
                while (isMoving) { 
                    moveTarget = (Vector2)playerTransform.position + (Vector2.up * 12);
                    yield return null; 
                }
                //flip over, wait a split second, and dive
                //flip

                //dive
                isMoving = true;
                moveTarget = new Vector2(transform.position.x, bottomRight.y);
                speedMod = 3;
                while (isMoving) { yield return null; }

                //unflip
                
                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        //move into next attack
        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Gust()
    {
        isMoving = true;

        switch (phase)
        {
            case Phase.one:
                //pick location (diagonal above player, keep updating)
                isMoving = true;
                speedMod = 1;
                int dir = (playerTransform.position.x > transform.position.x) ? -1 : 1;
                while (isMoving)
                {
                    moveTarget = (Vector2)playerTransform.position + (Vector2.up * 6) + (Vector2.right * 12 
                        * dir);
                    yield return null;
                }

                //shoot gust towards ground in front of player where it will move forward
                gustInst.gameObject.SetActive(true);
                gustInst.Reset(new Vector2(transform.position.x, transform.position.y), 
                    new Vector2(playerTransform.position.x + ((playerTransform.position.x > transform.position.x) ? -4 : 4),
                    playerTransform.position.y - playerGroundOffset));

                yield return new WaitForSeconds(0.3f);
                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Rain()
    {
        switch (phase)
        {
            case Phase.one:
                //top right or top left corner
                int dir;
                if(transform.position.x > ((bottomRight.x - topLeft.x)/2 + topLeft.x))
                {
                    moveTarget.x = bottomRight.x - 4;
                    dir = -1;
                }
                else
                {
                    moveTarget.x = topLeft.x + 4;
                    dir = 1;
                }
                moveTarget.y = topLeft.y - 4;
                speedMod = 2.5f;
                isMoving = true;
                while (isMoving) { yield return null; }


                //move target = opposite top corner
                moveTarget.x = (dir == 1) ? bottomRight.x - 4 : topLeft.x + 4;
                speedMod = 2f;
                isMoving = true;
                //shoot feathers diagonal towards the ground
                while (isMoving)
                {
                    HorusFeatherScript feather = disabledFeathers[0];
                    feather.gameObject.SetActive(true);
                    disabledFeathers.RemoveAt(0);
                    enabledFeathers.Add(feather);
                    feather.Reset(transform.position, new Vector2(0.33f * dir , -1).normalized);
                    yield return new WaitForSeconds(rainShotDelay);
                }

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Shotgun()
    {
        switch (phase)
        {
            case Phase.one:
                //go somewhere just above player
                float xSpacing = rng.Next(-10, 9) + (float)rng.NextDouble();
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving)
                {
                    moveTarget.x = playerTransform.position.x + xSpacing;
                    moveTarget.y = playerTransform.position.y + 10;
                    yield return null;
                }

                //shotgun of feathers
                float angleToPlayer = Mathf.Atan2(playerTransform.position.y - transform.position.y,
                    playerTransform.position.x - transform.position.x) * Mathf.Rad2Deg;
                yield return new WaitForSeconds(0.1f);//pause so player can dodge
                for(int i = 0; i < shotgunCount; i++)
                {
                    float degVariance = rng.Next(-shotgunConeDeg, shotgunConeDeg - 1) + (float)rng.NextDouble();


                    HorusFeatherScript feather = disabledFeathers[0];
                    feather.gameObject.SetActive(true);
                    disabledFeathers.RemoveAt(0);
                    enabledFeathers.Add(feather);
                    feather.Reset(transform.position, new Vector2(Mathf.Cos(Mathf.Deg2Rad * (degVariance + angleToPlayer)), 
                        Mathf.Sin(Mathf.Deg2Rad * (degVariance + angleToPlayer))));
                }

                yield return new WaitForSeconds(0.5f);

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator XAttack()
    {
        switch (phase)
        {
            case Phase.one:
                //wing attack; move close and attack after short pause
                int dir = 0;
                if (transform.position.x > playerTransform.position.x)
                {
                    dir = -1;
                }
                else
                {
                    dir = 1;
                }
                isMoving = true;
                speedMod = 1;
                ///update target
                while (isMoving)
                {
                    moveTarget = new Vector2(playerTransform.position.x + (2 * -dir), playerTransform.position.y + 2);
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);

                //wing attack (blend tree anim)


                break;
            case Phase.two:

                break;
            case Phase.three:
                //x attack from behind screen
                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Swoop()
    {
        switch (phase)
        {
            case Phase.one:
                //move close above player and then swoop with claws
                int dir = 0;
                if (transform.position.x > playerTransform.position.x)
                {
                    dir = -1;
                }
                else
                {
                    dir = 1;
                }
                isMoving = true;
                speedMod = 1;
                ///update target
                while (isMoving)
                {
                    moveTarget = new Vector2(playerTransform.position.x + (3 * -dir), playerTransform.position.y + 3);
                    yield return null;
                }
                
                //swoop (add extra attack hb in anim)
                //first segment
                moveTarget.x = playerTransform.position.x;
                moveTarget.y = playerTransform.position.y;
                speedMod = 1.25f;
                isMoving = true;
                while (isMoving) { yield return null; }
                //second segment
                moveTarget += new Vector2((3 * dir), 3);
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving) { yield return null; }

                yield return new WaitForSeconds(0.1f);

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }
}
