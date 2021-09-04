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
        wing
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

    Vector2 moveTarget = Vector2.zero;
    bool isMoving = false;

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
        /*List<Attack> possibleAttacks = new List<Attack>() { Attack.dive, Attack.gusts, Attack.rain, Attack.shotgun, Attack.wing, Attack.xAttack };
        possibleAttacks.Remove(prevAttack);
        CurrentAttack = possibleAttacks[rng.Next(0, possibleAttacks.Count - 1)];*/
        CurrentAttack = Attack.gusts;

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
            case Attack.wing:
                StartCoroutine(Wing());
                break;
            case Attack.xAttack:
                StartCoroutine(XAttack());
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

                yield return new WaitForSeconds(2f);
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
        isMoving = true;

        switch (phase)
        {
            case Phase.one:

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
        isMoving = true;

        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Wing()
    {
        isMoving = true;

        switch (phase)
        {
            case Phase.one:

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
        isMoving = true;

        switch (phase)
        {
            case Phase.one:

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
