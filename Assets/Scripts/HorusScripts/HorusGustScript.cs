using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusGustScript : MonoBehaviour
{
    public Vector2 groundTarget;

    public float initSpeed;
    public float timeToDespawn;

    float speed;
    bool grounded = false;
    int dir = 0;

    private void Start()
    {
        speed = initSpeed;
        dir = (groundTarget.x > transform.position.x) ? 1 : -1;
    }

    private void FixedUpdate()
    {
        if (grounded)
        {
            transform.position += Vector3.right * dir * speed * Time.deltaTime;
        }
        else
        {
            if(Vector2.Distance(groundTarget, transform.position) < speed * Time.deltaTime) 
            { 
                transform.position = groundTarget; 
                grounded = true; 
                Invoke("StartDisable", timeToDespawn);
            }
            else
            {
                Vector2 movement = (groundTarget - (Vector2)transform.position).normalized * speed * Time.deltaTime;
                transform.position += new Vector3(movement.x, movement.y, 0);
            }
        }
    }

    //For object recycling
    public void Reset(Vector2 setPos, Vector2 target)
    {
        speed = initSpeed;
        transform.position = setPos;
        groundTarget = target;
        dir = (groundTarget.x > transform.position.x) ? 1 : -1;
        grounded = false;
    }

    private void OnDisable()
    {
        transform.position = new Vector3(-100, -100, 0);
    }

    void StartDisable()
    {
        gameObject.SetActive(false);
    }
}
