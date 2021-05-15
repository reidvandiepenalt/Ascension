using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SlamAttackScript : MonoBehaviour
{
    public bool right = false;
    public float fallDistance;
    public float time = 1.5f;
    public float speed = 1.0f;
    private int damage;
    private float stepper = 0.0f;
    public float maxDamage = 100.0f;
    public float rate = 3.5f; //pushes right
    public float expRate = 0.2f; // lower = slower curve
    Rigidbody2D rb;
    public AttackInteract attackScript;
    // Start is called before the first frame update
    void Start()
    {
        CalculateDamage();
        rb = gameObject.GetComponent<Rigidbody2D>();
        if (!right)
        {
            gameObject.transform.localScale = new Vector3(-gameObject.transform.localScale.x, gameObject.transform.localScale.y, gameObject.transform.localScale.z);
            rb.velocity = new Vector2(-speed, 0);
        } else
        {
            rb.velocity = new Vector2(speed, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        stepper += Time.deltaTime;
        if (stepper > time)
        {
            Destroy(gameObject);
        }
    }

    void CalculateDamage()
    {
        damage = Mathf.RoundToInt(maxDamage / (1 + rate * (math.exp((fallDistance - 4) * -expRate))));
    }
}
