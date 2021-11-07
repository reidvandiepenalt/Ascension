using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusAI : MonoBehaviour
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
    List<Attack> attackOrder = new List<Attack>() { Attack.dive, Attack.gusts, Attack.rain, Attack.shotgun, Attack.xAttack, Attack.swoop };
    Attack CurrentAttack { get => curAttack;  set 
        { 
            attackOrder.Add(curAttack); 
            curAttack = value;
            attackOrder.Remove(value);
        } 
    }

    enum Phase
    {
        one,
        two,
        three
    }
    Phase phase = Phase.one;

    [SerializeField] GameObject gfx;
    [SerializeField] GameObject contactHB;

    [SerializeField] Animator diveFX;
    [SerializeField] GameObject tornado;
    [SerializeField] Animator crossAttack;

    [SerializeField] LayerMask playerLayer;
    [SerializeField] float diveTotalRadius;
    [SerializeField] float diveOffset;
    Collider2D playerCollider;

    [SerializeField] float tornadoFeatherSpacing;
    [SerializeField] float tornadoColumnSpacing;

    [SerializeField] HorusWingAttackScript wingAttack;

    public Vector2 topLeft;
    public Vector2 bottomRight;
    public LayerMask groundMask;

    bool ToRightOfPlayer { get => transform.position.x > playerTransform.position.x; }

    float CenterX { get => ((bottomRight.x - topLeft.x) / 2) + topLeft.x; }
    float CenterY { get => ((topLeft.y - bottomRight.y) / 2) + bottomRight.y; }
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

    public Transform playerTransform;
    float playerGroundOffset;

    [SerializeField] EnemyHealth healthManager;

    System.Random rng;

    bool facingRight = true;
    bool spiraling = false;

    public void OnStun(object param)
    {
        float time = (float)param;
        //stun for a given time

    }

    public void OnHit(object parameter)
    {
        int health = (int)parameter;
        if(health < 0)
        {
            Die();
        }else if(health < (healthManager.MaxHealth / 5f))
        {
            phase = Phase.three;
        }else if (health < (3 * healthManager.MaxHealth / 5f))
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
        rng = new System.Random();

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;

        phase = Phase.one;
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

                if (spiraling)
                {
                    return;
                }

                if (movement.x > 0 && !facingRight)
                {
                    transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    facingRight = true;
                }
                else if (movement.x < 0 && facingRight)
                {
                    transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    facingRight = false;
                }
            }
        }
    }

    /// <summary>
    /// Picks a new attack for horus
    /// </summary>
    void PickAttack()
    {
        //random new attack; weighted towards using attacks it hasnt used in a while
        int randomWeight = Random.Range(0, 1);
        if(randomWeight < 0.35f)
        {
            CurrentAttack = attackOrder[0];
        }else if (randomWeight < 0.65f)
        {
            CurrentAttack = attackOrder[1];
        }
        else if (randomWeight < 0.85f)
        {
            CurrentAttack = attackOrder[2];
        }
        else if (randomWeight < 0.95f)
        {
            CurrentAttack = attackOrder[3];
        }
        else
        {
            CurrentAttack = attackOrder[4];
        }

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

                yield return StartCoroutine(BasicDive());

                //pick location (below player, keep updating)
                moveTarget.y = playerTransform.position.y - 14;
                isMoving = true;
                speedMod = 1.5f;
                while (isMoving)
                {
                    moveTarget.x = playerTransform.position.x;
                    yield return null;
                }

                int dir = (Mathf.Sign(transform.localScale.x) < 0) ? -1 : 1;
                if (dir == 1)
                {
                    anim.SetBool("UpDiveRight", true);
                }
                else
                {
                    anim.SetBool("UpDiveLeft", true);
                }

                yield return new WaitForSeconds(0.1f);

                isMoving = true;
                moveTarget = new Vector2(transform.position.x, topLeft.y);
                speedMod = 3;
                while (isMoving) { yield return null; }

                if (dir == 1)
                {
                    anim.SetBool("UpDiveRight", false);
                }
                else
                {
                    anim.SetBool("UpDiveLeft", false);
                }

                yield return new WaitForSeconds(0.1f);

                break;
            case Phase.three:
                //aoe dive attacks
                //5 times in a row; tp to top 

                //move above player
                moveTarget.y = topLeft.y;
                isMoving = true;
                speedMod = 3;
                while (isMoving)
                {
                    moveTarget.x = playerTransform.position.x;
                    yield return null;
                }

                //start dives
                dir = (Mathf.Sign(transform.localScale.x) < 0) ? -1 : 1;
                if (dir == 1)
                {
                    anim.SetBool("DiveRight", true);
                }
                else
                {
                    anim.SetBool("DiveLeft", true);
                }
                for (int i = 0; i < 5; i++)
                {
                    yield return StartCoroutine(AOEDive());
                    transform.position = new Vector2(playerTransform.position.x, topLeft.y);
                }
                if (dir == 1)
                {
                    anim.SetBool("DiveRight", false);
                }
                else
                {
                    anim.SetBool("DiveLeft", false);
                }

                break;
        }
        //move into next attack
        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator AOEDive()
    {

        //dive
        isMoving = true;
        moveTarget = new Vector2(transform.position.x, bottomRight.y);
        speedMod = 3;
        while (isMoving) { yield return null; }

        //aoe attack; continue moving down
        isMoving = true;
        moveTarget = new Vector2(transform.position.x, bottomRight.y-20);
        speedMod = 3;

        diveFX.transform.position = (Vector2)transform.position + (Vector2.up * diveOffset);
        diveFX.gameObject.SetActive(true);
        diveFX.SetTrigger("Attack");
        yield return new WaitForEndOfFrame();
        float timer = 0;
        float totalTime = diveFX.GetCurrentAnimatorStateInfo(0).length;
        while (timer <= totalTime)
        {
            if(Vector2.Distance(playerCollider.ClosestPoint(diveFX.transform.position + Vector3.up * diveOffset), diveFX.transform.position + Vector3.up * diveOffset) < 
                (timer / totalTime) * diveTotalRadius)
            {
                playerTransform.GetComponent<PlayerTestScript>().TakeDamage(1, false);
            }
            timer += Time.deltaTime;
            yield return null;
        }

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
            moveTarget.x = playerTransform.position.x;
            moveTarget.y = topLeft.y;
            yield return null;
        }
        //flip over, and dive
        //flip
        int dir;
        if (Mathf.Sign(transform.localScale.x) >= 0) 
        {
            anim.SetBool("DiveRight", true);
            dir = 1;
        }
        else 
        {
            anim.SetBool("DiveLeft", true);
            dir = -1;
        }

        yield return new WaitForSeconds(0.1f);

        //dive
        isMoving = true;
        moveTarget = new Vector2(transform.position.x, bottomRight.y);
        speedMod = 3;
        while (isMoving) { yield return null; }

        //unflip
        if(dir == 1)
        {
            anim.SetBool("DiveRight", false);
        }
        else
        {
            anim.SetBool("DiveLeft", false);
        }
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
                moveTarget.x = transform.position.x;
                moveTarget.y = playerTransform.position.y;
                isMoving = true;
                speedMod = 1f;
                while (isMoving) { yield return null; }

                //dash through player
                bool Left;
                if (ToRightOfPlayer) { anim.SetBool("DashLeft", true); Left = true; } else
                {
                    anim.SetBool("DashRight", true);
                    Left = false;
                }
                
                
                moveTarget.x = playerTransform.position.x + ((ToRightOfPlayer) ? -6 : 6);
                isMoving = true;
                speedMod = 2.5f;
                while (isMoving) { yield return null; }
                if (Left)
                {
                    anim.SetBool("DashLeft", false);
                }
                else
                {
                    anim.SetBool("DashRight", false);
                }
                

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
                while (timer < 6f)
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
                    timer += 0.3f;
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
        int dir = (ToRightOfPlayer) ? -1 : 1;
        while (isMoving)
        {
            moveTarget = (Vector2)playerTransform.position + (Vector2.up * 6) + (Vector2.right * 12
                * dir);
            yield return null;
        }
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * -dir, transform.localScale.y, transform.localScale.z);
        facingRight = (dir == 1) ? false : true;

        //gust anim
        anim.SetTrigger("Gust");
        yield return new WaitForSeconds(0.15f);

        //shoot gust towards ground in front of player where it will move forward
        gustInst.gameObject.SetActive(true);
        gustInst.Reset(new Vector2(transform.position.x, transform.position.y),
            new Vector2(playerTransform.position.x + ((playerTransform.position.x > transform.position.x) ? -4 : 4),
            playerTransform.position.y - playerGroundOffset));

        yield return new WaitForSeconds(0.25f);
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
                while(isMoving) { yield return null; }

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
        moveTarget.x = (dir == -1) ? bottomRight.x - 4 : topLeft.x + 4;
        speedMod = 2f;
        isMoving = true;
        if(dir == -1) { anim.SetBool("DashRight", true); } else
        {
            anim.SetBool("DashLeft", true);
        }
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
        if (dir == -1) { anim.SetBool("DashRight", false); }
        else
        {
            anim.SetBool("DashLeft", false);
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
                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);

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

                anim.SetTrigger("Gust");
                //pause so player can dodge
                yield return new WaitForSeconds(0.15f);

                ShotgunAttack(0, false);

                yield return new WaitForSeconds(0.1f);
                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);

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

                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);//pause so player can dodge

                ShotgunAttack(0, true);

                yield return new WaitForSeconds(0.25f);
                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);

                ShotgunAttack(30, true);
                ShotgunAttack(-30, true);

                yield return new WaitForSeconds(0.25f);
                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);

                ShotgunAttack(30, true);
                ShotgunAttack(0, true);
                ShotgunAttack(-30, true);

                yield return new WaitForSeconds(0.5f);

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
            feather.Reset(transform.position + (Vector3.up * 3), new Vector2(Mathf.Cos(Mathf.Deg2Rad * (degVariance + angleToPlayer)),
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
                    moveTarget = new Vector2(playerTransform.position.x + (6.5f * -dir), playerTransform.position.y + 6.5f);
                    yield return null;
                }

                yield return new WaitForSeconds(0.1f);

                //wing attack (blend tree anim)
                Vector2 targetDir = ((Vector2)(playerTransform.position - transform.position)).normalized;
                anim.SetFloat("WingX", targetDir.x);
                anim.SetFloat("WingY", targetDir.y);
                anim.SetTrigger("WingAttack");
                yield return new WaitForSeconds(0.116f);
                //spawn attack
                Vector2 attackDir = ((Vector2)(playerTransform.position - transform.position)).normalized;
                wingAttack.Reset(transform.position, attackDir);

                yield return new WaitForSeconds(0.2f);

                break;
            case Phase.two:
                //series of moving attacks
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
                    moveTarget = new Vector2(playerTransform.position.x + (6.5f * -dir), playerTransform.position.y + 6.5f);
                    yield return null;
                }

                //3 attacks in a row
                for (int i = 0; i < 3; i++)
                {
                    //wing attack (blend tree anim)
                    targetDir = ((Vector2)(playerTransform.position - transform.position)).normalized;
                    anim.SetFloat("WingX", targetDir.x);
                    anim.SetFloat("WingY", targetDir.y);
                    anim.SetTrigger("WingAttack");
                    yield return new WaitForSeconds(0.116f);
                    //spawn attack
                    attackDir = ((Vector2)(playerTransform.position - transform.position)).normalized;
                    wingAttack.Reset(transform.position, attackDir);
                    yield return new WaitForSeconds(0.2f);

                    moveTarget.x = playerTransform.position.x;
                    moveTarget.y = playerTransform.position.y + 6.5f;
                    isMoving = true;
                    speedMod = 2f;
                    while(isMoving) { yield return null; }
                }

                yield return new WaitForSeconds(2f);

                break;
            case Phase.three:
                //x attack from behind screen

                //move up from center then scale horus down
                isMoving = true;
                speedMod = 1f;
                moveTarget.y = topLeft.y + RoomHeight;
                moveTarget.x = transform.position.x;
                while(isMoving) { yield return null; }

                //scale down
                transform.localScale = new Vector3(0.7f, 0.7f, 1);
                //disable hb
                contactHB.SetActive(false);

                //fly down to center
                transform.position = new Vector3(CenterX, transform.position.y, transform.position.z);
                isMoving = true;
                speedMod = 1f;
                moveTarget.y = CenterY;
                moveTarget.x = CenterX;
                while (isMoving) { yield return null; }

                //cross attack (through animator)
                crossAttack.gameObject.SetActive(true);
                crossAttack.transform.position = new Vector2(CenterX, CenterY);
                crossAttack.SetTrigger("Attack");
                yield return new WaitForSeconds(1.1f);

                //fly up and reset scale
                isMoving = true;
                speedMod = 1f;
                moveTarget.y = topLeft.y + RoomHeight;
                while (isMoving) { yield return null; }

                //scale up
                transform.localScale = new Vector3(1, 1, 1);
                contactHB.SetActive(true);

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
                yield return StartCoroutine(BasicSwoop(false));

                yield return new WaitForSeconds(0.1f);

                break;
            case Phase.two:
                //add shotgun after swoop

                yield return StartCoroutine(BasicSwoop(true));

                anim.SetTrigger("Gust");
                yield return new WaitForSeconds(0.15f);

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

                //for anims: rotate based on direction of movement?
                anim.SetBool("Spiral", true);
                anim.SetBool("Rotate", true);
                spiraling = true;
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                anim.SetFloat("RotateX", 0);
                anim.SetFloat("RotateY", 1);
                moveTarget.x = CenterX;
                moveTarget.y = CenterY;
                speedMod = 0.2f;
                isMoving = true;
                while (isMoving)
                {
                    if(Vector2.Distance(transform.position, moveTarget) < 1)
                    {
                        isMoving = false;
                        break;
                    }
                    Vector2 normalizedDif = ((Vector2)transform.position - moveTarget).normalized;
                    Vector2 translation = new Vector2(normalizedDif.y * 2 * ((CenterX / CenterY)), -normalizedDif.x) *
                        Mathf.Max(Vector2.Distance(transform.position, moveTarget), 4) * 3 * Time.deltaTime;
                    transform.Translate(translation, Space.World);
                    anim.SetFloat("RotateX", translation.x);
                    anim.SetFloat("RotateY", translation.y);
                    yield return null;
                }
                anim.SetBool("Spiral", false);
                anim.SetBool("Rotate", false);
                spiraling = false;
                
                break;
        }

        CurrentAttack = Attack.idle;
        yield break;
    }

    IEnumerator BasicSwoop(bool extendedEnd)
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

        anim.SetBool("Swoop", true);
        //first segment
        moveTarget.x = playerTransform.position.x + dir;
        moveTarget.y = playerTransform.position.y - (3 * playerGroundOffset / 4);
        speedMod = 1.25f;
        isMoving = true;
        while (isMoving) { yield return null; }
        //second segment
        moveTarget += new Vector2((3 * dir * ((extendedEnd) ? 2 : 1)), 3);
        isMoving = true;
        speedMod = 1.5f;
        while (isMoving) { yield return null; }
        anim.SetBool("Swoop", false);
    }
}
