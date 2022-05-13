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

    public void Begin(Vector2 startPos, float angRad, float speed = baseSpeed)
    {
        this.speed = speed;
        isSpawned = true;
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        transform.eulerAngles = new Vector3(0, 0, angRad * Mathf.Rad2Deg - 90);
        xMove = Mathf.Cos(angRad);
        yMove = Mathf.Sin(angRad);
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
