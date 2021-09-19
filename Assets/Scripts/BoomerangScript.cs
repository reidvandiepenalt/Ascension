using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoomerangScript : MonoBehaviour
{
    Rigidbody2D rb;
    public float despawnTime = 5.0f;
    private float timer = 0.0f;
    public float speed = 15.0f;
    public float stepper = 0.1f;
    public bool left = false;
    public float scale = 1.0f;
    [SerializeField] AttackInteract interact;

    // Start is called before the first frame update
    public void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        //set initial velocity based on direction
        if (left)
        {
            rb.velocity = new Vector2(-speed, 0);
        } else { rb.velocity = new Vector2(speed, 0);  }
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    public void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, 0, 4));

        timer += Time.deltaTime;

        //turn around
        if (left && timer >= 0.75f)
        {
            rb.velocity = new Vector2(rb.velocity.x + stepper, 0);
            interact.instanceIDs.Clear();
        } else if (timer >= 0.75f)
        {
            rb.velocity = new Vector2(rb.velocity.x - stepper, 0);
            interact.instanceIDs.Clear();
        }

        //despawn
        if (timer >= despawnTime)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //if it hits ground/wall, destory or rebound
        if (collision.CompareTag("Inanimate"))
        {
            if(timer < 0.80)
            {
                rb.velocity = new Vector2 (-rb.velocity.x, 0);
                interact.instanceIDs.Clear();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
