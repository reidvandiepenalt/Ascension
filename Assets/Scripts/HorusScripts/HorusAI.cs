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

    Vector2 moveTarget = Vector2.zero;
    bool isMoving = false;

    public float speed;

    public Transform playerTransform;

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
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        //Entry point
        if(CurrentAttack == Attack.idle)
        {
            PickAttack();
        }

        //movement handling
        if (isMoving)
        {
            //reached destination
            if(Vector2.Distance(transform.position, moveTarget) > speed * Time.deltaTime)
            {
                isMoving = false;
                transform.position = moveTarget;
            }
            //move toward destination
            else
            {
                Vector2 movement = ((Vector2)transform.position - moveTarget).normalized * speed * Time.deltaTime;
                transform.position += new Vector3(movement.x, movement.y, transform.position.z);
            }
        }


    }

    /// <summary>
    /// Picks a new attack for horus
    /// </summary>
    void PickAttack()
    {
        //random new attack; make smarter?
        List<Attack> possibleAttacks = new List<Attack>() { Attack.dive, Attack.gusts, Attack.rain, Attack.shotgun, Attack.wing, Attack.xAttack };
        possibleAttacks.Remove(prevAttack);
        CurrentAttack = possibleAttacks[rng.Next(0, possibleAttacks.Count - 1)];

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
        //pick location (above player, keep updating)
        isMoving = true;
        moveTarget = (Vector2)playerTransform.position + (Vector2.up * 8);

        switch (phase)
        {
            case Phase.one:
                while (isMoving) { moveTarget = (Vector2)playerTransform.position + (Vector2.up * 8); yield return null; }
                //flip over, wait a split second, and dive
                //flip
                //dive
                isMoving = true;
                moveTarget = Physics2D.Raycast(transform.position, Vector2.down, Mathf.Infinity, groundMask).point;
                while (isMoving) { }
                yield break;
            case Phase.two:

                yield break;
            case Phase.three:

                yield break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator Gust()
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
