using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRainFireball : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float speed;
    [SerializeField] FloorFire floorFire;
    [SerializeField] AudioSource spawnSFX, loopSFX;
    bool spawned = false;


    private void FixedUpdate()
    {
        if (!spawned) { return; }

        float dist = speed * Time.fixedDeltaTime;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, dist + 0.75f, groundLayer); // need to include offset of origin
        if (hit)
        {
            if (hit.collider.gameObject.CompareTag("Inanimate"))
            {
                floorFire.Begin(hit.point);
                End();
            }
        }
        transform.position += Vector3.down * dist;
    }

    public void Begin(Vector2 startPos)
    {
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        spawned = true;
        spawnSFX.Play();
        loopSFX.Play();
    }

    void End()
    {
        transform.position = new Vector3(-80, -80, transform.position.z);
        spawned = false;
        loopSFX.Stop();
    }
}
