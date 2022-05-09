using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RapidFireFireball : MonoBehaviour
{
    bool isSpawned = false;
    const float baseSpeed = 18f;
    float speed;
    float xMove, yMove;

    private void Start()
    {
        speed = baseSpeed;
    }

    private void FixedUpdate()
    {
        if (!isSpawned) { return; }

        transform.position += new Vector3(speed * Time.fixedDeltaTime * xMove, speed * Time.fixedDeltaTime * yMove);
    }

    public void Begin(Vector2 startPos, Vector2 target, float speed = baseSpeed)
    {
        this.speed = speed;
        isSpawned = true;
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        float angle = Vector2.SignedAngle(startPos, target);
        xMove = Mathf.Cos(angle * Mathf.Deg2Rad);
        yMove = Mathf.Sin(angle * Mathf.Deg2Rad);
    }

    void End()
    {
        isSpawned = false;
        transform.position = new Vector3(-80, -80, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Inanimate") || collision.CompareTag("NonDestroyingGround"))
        {
            End();
        }
    }
}
