using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NovaTutorialAI : MonoBehaviour
{
    public Vector2[] roomBounds = new Vector2[2];
    public Transform playerTransform, leftArmSolver, rightArmSolver, rightLaserSpawn, leftLaserSpawn;
    public GameObject afterImage;

    public GameObject directedLaserPrefab, projectilePrefab, vertLaserPrefab;
    public Collider2D playerCollider, contactCollider;
    public Animator anim;
    public Transform target;

    public LayerMask ground;
    public float speed;
    public float arrivalAttackDelay, attackFinishDelay;
    public float directedProjDelay, directedProjSpeed;
    public float omniProjSpeed, omniDegRange;
    public int omniProjCount, vertLaserCount, directedProjCount;
    public float laserDelay, laserExistTime;
    public float vertLaserSpawnHeight, vertLaserSpawnInterval;

    public TutorialCutscene cutscene;

    Queue<Vector2> path;
    int currentWaypoint;
    bool reachedEndOfPath;

    float dist { get { return (transform.position - target.position).magnitude; } }
    float RoomWidth { get { return roomBounds[1].x - roomBounds[0].x; } }
    float RoomHeight { get { return roomBounds[0].y - roomBounds[1].y; } }

    System.Random rng;

    AIState state = AIState.idle;
    AIState State { get => state; set
        {
            prevState = state;
            state = value;
        } }
    AIState prevState = AIState.idle;

    enum AIState
    {
        idle,
        vertLaser,
        directedLaser,
        omniProj,
        directedProj,
    }

    /// <summary>
    /// Decide next attack and where to move
    /// </summary>
    protected void PickAttack()
    {
        //random attack
        List<AIState> possibleAttacks = new List<AIState>();
        //add possible states to states list
        //grounded =/=> grounded
        //cannot repeat
        switch (prevState)
        {
            case AIState.idle:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.directedLaser);
                possibleAttacks.Add(AIState.omniProj);
                possibleAttacks.Add(AIState.directedProj);
                break;
            case AIState.vertLaser:
                possibleAttacks.Add(AIState.directedLaser);
                possibleAttacks.Add(AIState.omniProj);
                possibleAttacks.Add(AIState.directedProj);
                break;
            case AIState.directedLaser:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.omniProj);
                possibleAttacks.Add(AIState.directedProj);
                break;
            case AIState.omniProj:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.directedLaser);
                possibleAttacks.Add(AIState.directedProj);
                break;
            case AIState.directedProj:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.omniProj);
                possibleAttacks.Add(AIState.directedLaser);
                break;
        }
        //random attack
        State = possibleAttacks[rng.Next(0, possibleAttacks.Count)];

        switch (State)
        {
            case AIState.idle:
                break;
            case AIState.vertLaser:
                StartCoroutine(VertLaser());
                break;
            case AIState.directedLaser:
                StartCoroutine(DirectedLaser());
                break;
            case AIState.omniProj:
                StartCoroutine(OmniProj());
                break;
            case AIState.directedProj:
                StartCoroutine(DirectedProj());
                break;
        }
    }

    /// <summary>
    /// Vertical laser attack
    /// </summary>
    IEnumerator VertLaser()
    {
        //move to top middle
        target.position = new Vector3(roomBounds[0].x + (RoomWidth / 2), roomBounds[0].y);

        GeneratePath();

        yield return new WaitWhile(() => !reachedEndOfPath);

        //spawn attack and animate
        afterImage.SetActive(false);
        SetInvincible(false);
        anim.SetBool("RaiseArms", true);
        yield return new WaitForSeconds(arrivalAttackDelay);
        bool leftToRight = (playerTransform.position.x > transform.position.x);
        for(int i = 0; i < vertLaserCount; i++)
        {
            NovaVertLaser ls;
            if (leftToRight)
            {
                ls = Instantiate(vertLaserPrefab, new Vector2(roomBounds[0].x + i * (RoomWidth / vertLaserCount),
                vertLaserSpawnHeight), Quaternion.identity).GetComponent<NovaVertLaser>();
            }
            else
            {
                ls = Instantiate(vertLaserPrefab, new Vector2(roomBounds[1].x - i * (RoomWidth / vertLaserCount),
                vertLaserSpawnHeight), Quaternion.identity).GetComponent<NovaVertLaser>();
            }
            
            ls.activationTime = laserDelay;
            ls.activeTime = laserExistTime;
            yield return new WaitForSeconds(vertLaserSpawnInterval);
        }
        anim.SetBool("RaiseArms", false);
        yield return new WaitForSeconds(attackFinishDelay);
        afterImage.SetActive(true);
        SetInvincible(true);

        State = AIState.idle;
    }

    /// <summary>
    /// Directed laser attack
    /// </summary>
    IEnumerator DirectedLaser()
    {
        //min 8 from player
        int y = rng.Next((int)roomBounds[1].y, (int)roomBounds[0].y);
        int xminOffset = (int)Math.Sqrt(Math.Abs(Math.Pow(8, 2) - Math.Pow(y - playerTransform.position.y, 2)));
        int x;
        if((int)playerTransform.position.x - xminOffset < (int)roomBounds[0].x)
        {
            x = rng.Next((int)playerTransform.position.x + xminOffset, (int)roomBounds[1].x);
        }
        else if ((int)playerTransform.position.x + xminOffset > (int)roomBounds[1].x)
        {
            x = rng.Next((int)roomBounds[0].x, (int)playerTransform.position.x - xminOffset);
        }
        else
        {
            if (rng.Next(0, 2) == 0)
            {
                x = rng.Next((int)roomBounds[0].x, (int)playerTransform.position.x - xminOffset);
            }
            else
            {
                x = rng.Next((int)playerTransform.position.x + xminOffset, (int)roomBounds[1].x);
            }
        }
        target.position = new Vector2(x, y);

        //create path
        GeneratePath();

        yield return new WaitWhile(() => !reachedEndOfPath);

        //animate
        afterImage.SetActive(false);
        SetInvincible(false);
        bool left = playerTransform.position.x <= transform.position.x;
        anim.SetBool("Laser", true);
        anim.SetFloat("LaserX", (playerCollider.bounds.center - transform.position).normalized.x);
        anim.SetFloat("LaserY", (playerCollider.bounds.center - transform.position).normalized.y);

        yield return new WaitForSeconds(arrivalAttackDelay);

        //spawn laser
        Vector3 laserSpawn = left? leftLaserSpawn.position : rightLaserSpawn.position;
        NovaDirectedLaser ls = Instantiate(directedLaserPrefab, laserSpawn, Quaternion.identity).GetComponent<NovaDirectedLaser>();
        ls.targetPosition = playerCollider.bounds.center;
        ls.initPosition = laserSpawn;
        
        yield return new WaitForSeconds(laserDelay);
        ls.DamageActive();
        yield return new WaitForSeconds(laserExistTime);
        Destroy(ls.gameObject);

        //reset anim
        anim.SetBool("Laser", false);

        yield return new WaitForSeconds(attackFinishDelay);
        afterImage.SetActive(true);
        SetInvincible(true);

        State = AIState.idle;
    }

    /// <summary>
    /// Omnidirectional projectile attack
    /// </summary>
    IEnumerator OmniProj()
    {
        //min 8 from player in top section
        int y = rng.Next((int)roomBounds[0].y / 2, (int)roomBounds[0].y);
        int xminOffset = (int)Math.Sqrt(Math.Abs(Math.Pow(8, 2) - Math.Pow(y - playerTransform.position.y, 2)));
        int x;
        if ((int)playerTransform.position.x - xminOffset < (int)roomBounds[0].x)
        {
            x = rng.Next((int)playerTransform.position.x + xminOffset, (int)roomBounds[1].x);
        }
        else if ((int)playerTransform.position.x + xminOffset > (int)roomBounds[1].x)
        {
            x = rng.Next((int)roomBounds[0].x, (int)playerTransform.position.x - xminOffset);
        }
        else
        {
            if (rng.Next(0, 2) == 0)
            {
                x = rng.Next((int)roomBounds[0].x, (int)playerTransform.position.x - xminOffset);
            }
            else
            {
                x = rng.Next((int)playerTransform.position.x + xminOffset, (int)roomBounds[1].x);
            }
        }
        target.position = new Vector2(x, y);

        //create path
        GeneratePath();

        yield return new WaitWhile(() => !reachedEndOfPath);

        //spawn attack and animate
        //spawn projectiles in a cone evenly spaced
        afterImage.SetActive(false);
        SetInvincible(false);
        yield return new WaitForSeconds(arrivalAttackDelay);
        for(int i = -omniProjCount / 2; i <= omniProjCount / 2; i++)
        {
            NovaProjectileScript proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<NovaProjectileScript>();
            proj.speed = directedProjSpeed;
            proj.DestroyOnWall = true;

            float sin = Mathf.Sin(omniDegRange / omniProjCount * i * Mathf.Deg2Rad);
            float cos = Mathf.Cos(omniDegRange / omniProjCount * i * Mathf.Deg2Rad);
            float projX =  sin;
            float projY = -cos;
            proj.targetPosition = Physics2D.Raycast(transform.position, new Vector2(projX, projY), 60, ground).point;
            StartCoroutine(proj.DirectedProj());
        }
        yield return new WaitForSeconds(attackFinishDelay);
        afterImage.SetActive(true);
        SetInvincible(true);


        State = AIState.idle;
    }

    /// <summary>
    /// Directed projectile attack
    /// </summary>
    IEnumerator DirectedProj()
    {
        //aerial vs grounded for real version?

        //left or right floor
        if (playerTransform.position.x < roomBounds[0].x + (RoomWidth / 2))
        {
            target.transform.position = roomBounds[1];
        }
        else
        {
            target.transform.position = new Vector2(roomBounds[0].x, roomBounds[1].y);
        }

        //create path
        GeneratePath();

        //traverse path (in update)
        yield return new WaitWhile(() => !reachedEndOfPath);

        //spawn attack and animate
        afterImage.SetActive(false);
        SetInvincible(false);
        yield return new WaitForSeconds(arrivalAttackDelay);
        for(int i = 0; i < directedProjCount; i++)
        {
            NovaProjectileScript proj = Instantiate(projectilePrefab, transform.position + (Vector3.up), Quaternion.identity).GetComponent<NovaProjectileScript>();
            proj.speed = directedProjSpeed;
            proj.DestroyOnWall = true;
            if (playerTransform.position.x > transform.position.x)
            {
                proj.targetPosition = Physics2D.Raycast(proj.transform.position, Vector2.right, 60, ground).point;
            }
            else
            {
                proj.targetPosition = Physics2D.Raycast(proj.transform.position, Vector2.left, 60, ground).point;
            }
            StartCoroutine(proj.DirectedProj());
            yield return new WaitForSeconds(directedProjDelay);
        }
        yield return new WaitForSeconds(attackFinishDelay);
        afterImage.SetActive(true);
        SetInvincible(true);

        State = AIState.idle;
    }

    /// <summary>
    /// Creates a path based on a quadratic curve from position to target position
    /// </summary>
    void GeneratePath()
    {
        reachedEndOfPath = false;
        //final x closer to player
        if (Math.Abs(target.position.x - playerTransform.position.x) < Math.Abs(transform.position.x - playerTransform.position.x))
        {
            Vector2 vertex = transform.position;
            Vector2 endpoint = target.position;
            float timeToMove = dist / speed;
            int numSteps = (int)(60f * timeToMove);
            float stepDist = (endpoint.y - vertex.y) / numSteps;

            //div by 0 case
            if (endpoint.y - vertex.y == 0)
            {
                float xStepDist = (endpoint.x - vertex.x) / numSteps;
                for (int i = 0; i < numSteps; i++)
                {
                    path.Enqueue(new Vector2(i * xStepDist + vertex.x, vertex.y));
                }
                return;
            }

            float a = (float)((endpoint.x - vertex.x) / Math.Pow(endpoint.y - vertex.y, 2));
            for (int i = 0; i < numSteps; i++)
            {
                path.Enqueue(new Vector2((float)(a * Math.Pow(i * stepDist, 2) + vertex.x), vertex.y + i * stepDist));
            }
        }
        //final x farther from player
        else
        {
            Vector2 vertex = transform.position;
            Vector2 endpoint = target.position;
            float timeToMove = dist / speed;
            int numSteps = (int)(60f * timeToMove);
            float stepDist = (endpoint.x - vertex.x) / numSteps;

            //div by 0 case
            if (endpoint.x - vertex.x == 0)
            {
                float yStepDist = (endpoint.y - vertex.y) / numSteps;
                for (int i = 0; i < numSteps; i++)
                {
                    path.Enqueue(new Vector2(vertex.x, i * yStepDist + vertex.y));
                }
                return;
            }

            float a = (float)((endpoint.y - vertex.y) / (Math.Pow(endpoint.x - vertex.x, 2)));
            for (int i = 0; i < numSteps; i++)
            {
                path.Enqueue(new Vector2(vertex.x + i * stepDist, (float)(a * Math.Pow(i * stepDist, 2) + vertex.y)));
            }
        }
    }

    // Start is called before the first frame update
    protected void Start()
    {
        ground = LayerMask.GetMask("Ground");
        reachedEndOfPath = false;
        path = new Queue<Vector2>();
        currentWaypoint = 0;
        rng = new System.Random();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<BoxCollider2D>();
    }

    protected void FixedUpdate()
    {
        //if paused, do nothing
        if (Pause.IsPaused) { return; }

        //catch case
        if (path == null)
        {
            PickAttack();
            return;
        }
        //attack over; pick new one
        if (path.Count == 0)
        {
            reachedEndOfPath = true;

            if (State == AIState.idle) { PickAttack(); }

            return;
        }
        //move
        transform.position = path.Dequeue();
        currentWaypoint++;
    }

    /// <summary>
    /// Turn the boss invincible or back to vincible
    /// </summary>
    /// <param name="on"></param>
    void SetInvincible(bool on)
    {
        if (on)
        {
            contactCollider.enabled = false;
        }
        else
        {
            contactCollider.enabled = true;
        }
    }

    /// <summary>
    /// Takes damage amount
    /// </summary>
    /// <param name="damageAmount"></param>
    public void OnHit(object param)
    {
        int health = (int)param;

        if(health < 0)
        {
            Cutscene();
        } 
    }

    /// <summary>
    /// Setup and play the intro cutscene
    /// </summary>
    private void Cutscene()
    {
        cutscene.gameObject.SetActive(true);
        cutscene.PlayCutscene();
        throw new NotImplementedException();
    }

    /// <summary>
    /// Stuns enemy
    /// </summary>
    /// <param name="time"></param>
    public void OnStun(object param)
    {
        float stunTime = (float)param;
        throw new NotImplementedException();
    }
}
