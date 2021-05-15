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

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (left)
        {
            rb.velocity = new Vector2(-speed, 0);
        } else { rb.velocity = new Vector2(speed, 0);  }
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(new Vector3(0, 0, 4));

        timer += Time.deltaTime;

        if (left && timer >= 0.75f)
        {
            rb.velocity = new Vector2(rb.velocity.x + stepper, 0);
        } else if (timer >= 0.75f)
        {
            rb.velocity = new Vector2(rb.velocity.x - stepper, 0);
        }

        if (timer >= despawnTime)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Inanimate"))
        {
            if(timer < 0.80)
            {
                rb.velocity = new Vector2 (-rb.velocity.x, 0);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
