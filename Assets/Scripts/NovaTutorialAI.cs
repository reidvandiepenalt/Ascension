using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NovaTutorialAI : MonoBehaviour, IEnemy
{
    public Vector2[] roomBounds = new Vector2[2];
    public Transform playerTransform;

    public GameObject laserPrefab, projectilePrefab;
    public Transform target;

    public LayerMask ground;
    public float timeToMove;
    public float arrivalAttackDelay, attackFinishDelay;
    public float directedProjDelay, directedProjSpeed;
    public float omniProjSpeed, omniDegRange;
    public int omniProjCount;
    public float laserDelay, laserExistTime;

    Queue<Vector2> path;
    int currentWaypoint;
    bool reachedEndOfPath;

    float RoomWidth { get { return roomBounds[1].x - roomBounds[0].x; } }
    float RoomHeight { get { return roomBounds[0].y - roomBounds[1].y; } }

    protected int health;
    protected int maxHealth;

    public int Health { get => health; set => health = value; }
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }

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
                break;
            case AIState.omniProj:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.directedLaser);
                possibleAttacks.Add(AIState.directedProj);
                break;
            case AIState.directedProj:
                possibleAttacks.Add(AIState.vertLaser);
                possibleAttacks.Add(AIState.omniProj);
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


        State = AIState.idle;
    }

    /// <summary>
    /// Directed laser attack
    /// </summary>
    IEnumerator DirectedLaser()
    {
        //grounded vs aerial for real version?

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

        yield return new WaitWhile(() => !reachedEndOfPath);

        //spawn attack and animate
        yield return new WaitForSeconds(arrivalAttackDelay);
        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        laser.GetComponent<Laser>().targetPosition = playerTransform.position;
        laser.SetActive(false);
        yield return new WaitForSeconds(laserDelay);
        laser.SetActive(true);
        laser.GetComponent<LineRenderer>().startColor = new Color(255, 255, 255, 255);
        yield return new WaitForSeconds(laserExistTime);
        Destroy(laser);
        yield return new WaitForSeconds(attackFinishDelay);

        State = AIState.idle;
    }

    /// <summary>
    /// Omnidirectional projectile attack
    /// </summary>
    IEnumerator OmniProj()
    {
        //pick a spot in top section away from player
        //player in left side
        if(playerTransform.position.x < roomBounds[1].x - (2 * RoomWidth / 3))
        {
            target.position = new Vector2(rng.Next((int)(roomBounds[1].x - (2 * RoomWidth / 3)), (int)roomBounds[1].x + 1) , rng.Next((int)(roomBounds[0].y - (RoomHeight / 2)), (int)roomBounds[0].y + 1));
        }
        //player in middle
        else if(playerTransform.position.x < roomBounds[1].x - (RoomWidth / 3))
        {
            if(rng.Next(0, 2) == 0)
            {
                target.position = new Vector2(rng.Next((int)roomBounds[0].x, (int)(roomBounds[0].x + (RoomWidth / 3)) + 1) , rng.Next((int)(roomBounds[0].y - (RoomHeight / 2)), (int)roomBounds[0].y + 1));
            }
            else
            {
                target.position = new Vector2(rng.Next((int)(roomBounds[1].x - (RoomWidth / 3)), (int)roomBounds[1].x + 1), rng.Next((int)(roomBounds[0].y - (RoomHeight / 2)), (int)roomBounds[0].y + 1));
            }
        }
        //player on right
        else
        {
            target.position = new Vector2(rng.Next((int)roomBounds[0].x, (int)(roomBounds[1].x - (2 * RoomWidth / 3)) + 1), rng.Next((int)(roomBounds[0].y - (RoomHeight / 2)), (int)roomBounds[0].y + 1));
        }

        //create path
        GeneratePath();

        yield return new WaitWhile(() => !reachedEndOfPath);

        //spawn attack and animate
        //spawn 7 projectiles in a 120 degree cone evenly spaced
        yield return new WaitForSeconds(arrivalAttackDelay);
        for(int i = -omniProjCount / 2; i <= omniProjCount / 2; i++)
        {
            NovaProjectileScript proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<NovaProjectileScript>();
            proj.speed = directedProjSpeed;
            proj.DestroyOnWall = true;

            float sin = Mathf.Sin(omniDegRange / omniProjCount * i * Mathf.Deg2Rad);
            float cos = Mathf.Cos(omniDegRange / omniProjCount * i * Mathf.Deg2Rad);
            float x =  sin;
            float y = -cos;
            proj.targetPosition = Physics2D.Raycast(transform.position, new Vector2(x, y), 60, ground).point;
            StartCoroutine(proj.DirectedProj());
        }
        yield return new WaitForSeconds(attackFinishDelay);

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
        yield return new WaitForSeconds(arrivalAttackDelay);
        for(int i = 0; i < 3; i++)
        {
            NovaProjectileScript proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<NovaProjectileScript>();
            proj.speed = directedProjSpeed;
            proj.DestroyOnWall = true;
            if (playerTransform.position.x > transform.position.x)
            {
                proj.targetPosition = Physics2D.Raycast(transform.position, Vector2.right, 60, ground).point;
            }
            else
            {
                proj.targetPosition = Physics2D.Raycast(transform.position, Vector2.left, 60, ground).point;
            }
            StartCoroutine(proj.DirectedProj());
            yield return new WaitForSeconds(directedProjDelay);
        }
        yield return new WaitForSeconds(attackFinishDelay);

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

        //dialogue?

        //pick attack
        PickAttack();
    }

    protected void FixedUpdate()
    {
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
    /// Takes damage amount
    /// </summary>
    /// <param name="damageAmount"></param>
    public void TakeDamage(int damageAmount)
    {
        Health -= damageAmount;

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
        throw new NotImplementedException();
    }

    /// <summary>
    /// Stuns enemy
    /// </summary>
    /// <param name="time"></param>
    public void Stun(float time)
    {
        throw new NotImplementedException();
    }
}
