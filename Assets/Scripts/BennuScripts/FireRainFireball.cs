using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireRainFireball : MonoBehaviour
{
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float speed;
    [SerializeField] FloorFire floorFire;
    bool spawned = false;


    private void FixedUpdate()
    {
        if(!spawned) { return; }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, speed * Time.fixedDeltaTime, groundLayer); // need to include offset of origin
        if (hit)
        {
            if (hit.collider.gameObject.CompareTag("Inanimate"))
            {
                floorFire.Begin(hit.point);
                End();
            }
        }
    }

    public void Begin(Vector2 startPos)
    {
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        spawned = true;
    }

    void End()
    {
        transform.position = new Vector3(-80, -80, transform.position.z);
        spawned = false;
    }
}
