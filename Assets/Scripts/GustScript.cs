using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GustScript : MonoBehaviour
{
    public float stunTime = 0.5f;
    public bool left = false;
    public float speed = 3.0f;
    public float time = 0.75f;
    private float timer = 0.0f;
    Rigidbody2D rb;
    List<int> instanceIDs;

    void OnTriggerEnter2D(Collider2D target)
    {
        //stun on hit
        if (target.gameObject.tag == "Enemy")
        {
            int instanceID = target.GetInstanceID();
            if (instanceIDs.Contains(instanceID)) { return; }
            instanceIDs.Add(instanceID);
            target.gameObject.GetComponent<EnemyCompositeHB>().Stun(stunTime);
        }
    }

    private void Start()
    {
        //set speed and flip if needed
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(speed, rb.velocity.y);
        if (left)
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
        }
    }

    private void Update()
    {
        //despawn
        timer += Time.deltaTime;
        if (timer > time)
        {
            Destroy(gameObject);
        }
    }
}
