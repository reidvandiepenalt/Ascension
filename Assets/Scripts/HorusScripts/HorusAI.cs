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


    Vector2 moveTarget = Vector2.zero;
    bool isMoving = false;

    public float speed;

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
    }

    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }

        if (isMoving)
        {
            if(Vector2.Distance(transform.position, moveTarget) > speed * Time.deltaTime)
            {
                isMoving = false;
                transform.position = moveTarget;
                //go to next attack?
            }
            else
            {
                Vector2 movement = ((Vector2)transform.position - moveTarget).normalized * speed * Time.deltaTime;
                transform.position += new Vector3(movement.x, movement.y, transform.position.z);
            }
        }


    }

    void PickAttack()
    {
        //random new attack; make smarter
        List<Attack> possibleAttacks = new List<Attack>() { Attack.dive, Attack.gusts, Attack.rain, Attack.shotgun, Attack.wing, Attack.xAttack };
        possibleAttacks.Remove(prevAttack);
        CurrentAttack = possibleAttacks[rng.Next(0, possibleAttacks.Count - 1)];


    }

    IEnumerator Dive()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }

    IEnumerator Gust()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }

    IEnumerator Rain()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }

    IEnumerator Shotgun()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }

    IEnumerator Wing()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }

    IEnumerator XAttack()
    {
        switch (phase)
        {
            case Phase.one:

                break;
            case Phase.two:

                break;
            case Phase.three:

                break;
        }
        yield break;
    }
}
