using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeatherScript : MonoBehaviour
{
    Rigidbody2D rb;
    public float despawnTime = 5.0f;
    private float timer = 0.0f;
    public float speed = 15.0f;
    public float angle = 0.0f;
    public float scale = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        //set up
        rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.transform.eulerAngles = new Vector3(0, 0, angle - 90);
        angle *= Mathf.Deg2Rad;
        rb.velocity = new Vector2(speed * Mathf.Cos(angle), speed * Mathf.Sin(angle));
        gameObject.transform.localScale = new Vector3(scale, scale, scale);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        //despawn
        if (timer > despawnTime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //despawn on ground/wall
        if (collision.CompareTag("Inanimate"))
        {
            Destroy(gameObject);
        }
    }
}
