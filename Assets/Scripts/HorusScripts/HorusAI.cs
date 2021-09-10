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

    [SerializeField]
    GameObject tornado;

    [SerializeField] float tornadoFeatherSpacing;
    [SerializeField] float tornadoColumnSpacing;

    public Vector2 topLeft;
    public Vector2 bottomRight;
    public LayerMask groundMask;

    bool ToRightOfPlayer { get => transform.position.x > playerTransform.position.x; }

    float CenterX { get => (bottomRight.x - topLeft.x) / 2 + topLeft.x; }
    float CenterY { get => (topLeft.y - bottomRight.y) / 2 + bottomRight.y; }
    float RoomHeight { get => topLeft.y - bottomRight.y; }

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

    public float circleShrinkSpeed;

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
        }else if(Health < (MaxHealth / 5f))
        {
            phase = Phase.three;
        }else if (Health < (3 * MaxHealth / 5f))
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

        phase = Phase.three;
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
        CurrentAttack = Attack.rain;

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
                yield return StartCoroutine(BasicDive());

                yield return new WaitForSeconds(0.1f);
                break;
            case Phase.two:
                StartCoroutine(BasicDive());

                //pick location (below player, keep updating)
                isMoving = true;
                speedMod = 3;
                while (isMoving)
                {
                    moveTarget = (Vector2)playerTransform.position + (Vector2.down * 12);
                    yield return null;
                }
                //flip over, wait a split second, and go up
                //flip

                //dive
                isMoving = true;
                moveTarget = new Vector2(transform.position.x, topLeft.y);
                speedMod = 3;
                while (isMoving) { yield return null; }
                //unflip

                break;
            case Phase.three:
                //aoe dive attacks


                break;
        }
        //move into next attack
        CurrentAttack = Attack.idle;
        yield break;
    }

    /// <summary>
    /// For resusability in various attacks of differing phases
    /// </summary>
    IEnumerator BasicDive()
    {
        //pick location (above player, keep updating)
        isMoving = true;
        speedMod = 1;
        while (isMoving)
        {
            moveTarget = (Vector2)playerTransform.position + (Vector2.up * 12);
            yield return null;
        }
        //flip over, and dive
        //flip

        //dive
        isMoving = true;
        moveTarget = new Vector2(transform.position.x, bottomRight.y);
        speedMod = 3;
        while (isMoving) { yield return null; }

        //unflip
    }

    IEnumerator Gust()
    {
        isMoving = true;

        switch (phase)
        {
            case Phase.one:
                yield return StartCoroutine(BasicGust());

                yield return new WaitForSeconds(0.3f);
                break;
            case Phase.two:
                //gust then horizontal flying attack?
                yield return StartCoroutine(BasicGust());

                //move to side
                moveTarget.x = playerTransform.position.x + ((ToRightOfPlayer) ? 6 : -6);
                moveTarget.y = playerTransform.position.y;
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving) { yield return null; }

                //dash through player
                moveTarget.x = playerTransform.position.x + ((ToRightOfPlayer) ? -6 : 6);
                isMoving = true;
                speedMod = 2.5f;
                while (isMoving) { yield return null; }

                break;
            case Phase.three:
                //create tornado in middle of room that pulls and shoots feathers?
                //move to middle
                moveTarget.x = CenterX;
                moveTarget.y = CenterY + (RoomHeight / 4);
                speedMod = 1f;
                isMoving = true;
                while (isMoving) { yield return null; }

                //spawn tornado
                tornado.SetActive(true);
                tornado.transform.position = new Vector2(CenterX, bottomRight.y);

                //spawn feathers in columns of 3 to either side that increase in height
                float timer = 0f;
                int heightLevel = 0;
                while (timer < 8f)
                {
                    for(int i = 1; i <= 3; i++)
                    {
                        HorusFeatherScript leftFeather = disabledFeathers[0];
                        leftFeather.gameObject.SetActive(true);
                        disabledFeathers.RemoveAt(0);
                        enabledFeathers.Add(leftFeather);
                        leftFeather.Reset(new Vector2(CenterX, bottomRight.y + (tornadoFeatherSpacing * i + tornadoColumnSpacing * heightLevel)),
                            new Vector2(-1, 0).normalized, false);
                        HorusFeatherScript rightFeather = disabledFeathers[0];
                        rightFeather.gameObject.SetActive(true);
                        disabledFeathers.RemoveAt(0);
                        enabledFeathers.Add(rightFeather);
                        rightFeather.Reset(new Vector2(CenterX, bottomRight.y + (tornadoFeatherSpacing * i + tornadoColumnSpacing * heightLevel)),
                            new Vector2(1, 0).normalized, false);
                    }
                    heightLevel++;
                    if(heightLevel >= RoomHeight / tornadoColumnSpacing) { heightLevel = 0; }
                    timer += Time.deltaTime;
                    yield return new WaitForSeconds(0.3f);
                }

                //despawn tornado
                tornado.transform.position = new Vector2(-100, -100);
                tornado.SetActive(false);

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator BasicGust()
    {
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
    }

    IEnumerator Rain()
    {
        switch (phase)
        {
            case Phase.one:
                //pick corner
                int dir = (transform.position.x > CenterX) ? 1 : -1;
                moveTarget = new Vector2((dir == 1) ? bottomRight.x - 4 : moveTarget.x = topLeft.x + 4, topLeft.y);
                isMoving = true;
                speedMod = 2.5f;
                while (isMoving) { yield return null; }

                //start passes
                yield return StartCoroutine(RainAttack(dir, false));

                yield return new WaitForSeconds(0.1f);
                break;
            case Phase.two:
                //go across twice
                dir = (transform.position.x > CenterX) ? 1 : -1;
                moveTarget = new Vector2((dir == 1) ? bottomRight.x - 4 : moveTarget.x = topLeft.x + 4, topLeft.y);
                isMoving = true;
                speedMod = 2.5f;
                while (isMoving) { yield return null; }

                //start passes
                yield return StartCoroutine(RainAttack(dir, false));

                //opposite corner
                dir = -dir;
                //stecond pass
                yield return StartCoroutine(RainAttack(dir, false));

                yield return new WaitForSeconds(0.1f);
                break;
            case Phase.three:
                //replace rain with shotguns? homing shots? shots coming from floor?
                //try bounce
                //top corner
                dir = (transform.position.x > CenterX) ? 1 : -1;
                moveTarget = new Vector2((dir == 1) ? bottomRight.x - 4 : moveTarget.x = topLeft.x + 4, topLeft.y);
                isMoving = true;
                speedMod = 2.5f;

                //go across twice
                dir = (transform.position.x > CenterX) ? 1 : -1;
                yield return StartCoroutine(RainAttack(dir, true));
                //opposite corner
                dir = -dir;
                yield return StartCoroutine(RainAttack(dir, true));

                yield return new WaitForSeconds(0.1f);

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator RainAttack(int dir, bool bounce)
    {
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
            feather.Reset(transform.position, new Vector2(0.4f * dir, -1).normalized, bounce);
            yield return new WaitForSeconds(rainShotDelay);
        }
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
                yield return new WaitForSeconds(0.1f);//pause so player can dodge

                ShotgunAttack(0, false);

                yield return new WaitForSeconds(0.2f);

                break;
            case Phase.two:
                //shoot 1 then 2 in a v
                //go somewhere just above player
                xSpacing = rng.Next(-10, 9) + (float)rng.NextDouble();
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving)
                {
                    moveTarget.x = playerTransform.position.x + xSpacing;
                    moveTarget.y = playerTransform.position.y + 10;
                    yield return null;
                }
                yield return new WaitForSeconds(0.1f);//pause so player can dodge

                ShotgunAttack(0, false);

                yield return new WaitForSeconds(0.1f);

                ShotgunAttack(30, false);
                ShotgunAttack(-30, false);

                yield return new WaitForSeconds(0.2f);

                break;
            case Phase.three:
                //1 bounce shotguns? homing shots?
                //shoot 1 then 2 then 3 and make bounce
                //go somewhere just above player
                xSpacing = rng.Next(-10, 9) + (float)rng.NextDouble();
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving)
                {
                    moveTarget.x = playerTransform.position.x + xSpacing;
                    moveTarget.y = playerTransform.position.y + 10;
                    yield return null;
                }
                yield return new WaitForSeconds(0.1f);//pause so player can dodge

                ShotgunAttack(0, true);

                yield return new WaitForSeconds(0.1f);

                ShotgunAttack(30, true);
                ShotgunAttack(-30, true);

                yield return new WaitForSeconds(0.1f);

                ShotgunAttack(30, true);
                ShotgunAttack(0, true);
                ShotgunAttack(-30, true);

                yield return new WaitForSeconds(0.2f);

                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    /// <summary>
    /// Helper method for shotgun style attacks
    /// </summary>
    void ShotgunAttack(float rotation, bool bounce)
    {
        //shotgun of feathers
        float angleToPlayer = (Mathf.Atan2(playerTransform.position.y - transform.position.y,
            playerTransform.position.x - transform.position.x) * Mathf.Rad2Deg) + rotation;
        for (int i = 0; i < shotgunCount; i++)
        {
            float degVariance = rng.Next(-shotgunConeDeg, shotgunConeDeg - 1) + (float)rng.NextDouble();


            HorusFeatherScript feather = disabledFeathers[0];
            feather.gameObject.SetActive(true);
            disabledFeathers.RemoveAt(0);
            enabledFeathers.Add(feather);
            feather.Reset(transform.position, new Vector2(Mathf.Cos(Mathf.Deg2Rad * (degVariance + angleToPlayer)),
                Mathf.Sin(Mathf.Deg2Rad * (degVariance + angleToPlayer))), bounce);
        }
        return;
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
                //series of moving attacks

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
                yield return StartCoroutine(BasicSwoop());

                yield return new WaitForSeconds(0.1f);

                break;
            case Phase.two:
                //add shotgun after swoop

                yield return StartCoroutine(BasicSwoop());

                yield return new WaitForSeconds(0.1f);

                //shotgun after
                ShotgunAttack(0, false);


                break;
            case Phase.three:
                //room spiraling attack
                //move to top center at radius
                isMoving = true;
                speedMod = 2f;
                moveTarget.y = topLeft.y;
                moveTarget.x = CenterX;
                while (isMoving)
                {
                    yield return null;
                }

                //spiral
                //update will move toward center, while this moves tangent to circle
                moveTarget.x = CenterX;
                moveTarget.y = CenterY;
                speedMod = 0.15f;
                isMoving = true;
                while (isMoving)
                {
                    if(Vector2.Distance(transform.position, moveTarget) < 1)
                    {
                        isMoving = false;
                        break;
                    }
                    Vector2 normalizedDif = ((Vector2)transform.position - moveTarget).normalized;
                    transform.Translate( new Vector2(normalizedDif.y * 2 * ((CenterX/CenterY)), -normalizedDif.x) * 
                        Mathf.Max(Vector2.Distance(transform.position, moveTarget), 4) * 3 * Time.deltaTime);
                    yield return null;
                }
                
                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator BasicSwoop()
    {
        //move close above player and then swoop with claws
        int dir;
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
    }
}
