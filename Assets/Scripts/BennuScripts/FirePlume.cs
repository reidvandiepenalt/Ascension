using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlume : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] FirePlumeFireball[] fireballs;
    [SerializeField] FloorFire[] floorFires;

    public void Begin(Vector2 position, BennuAI.Phase phase)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        anim.SetTrigger("Plume");
        
        if(phase == BennuAI.Phase.two)
        {
            Invoke("SpawnFireballs", 11f/20f);
        }

        Invoke("End", 7f/12f);
    }

    void SpawnFireballs()
    {
        for(int i = 0; i < fireballs.Length; i++)
        {
            fireballs[i].Launch(transform.position + Vector3.up * 1.5f, 45 + 30 * i);
        }
    }

    void End()
    {
        for(int i = 0; i < floorFires.Length; i++)
        {
            floorFires[i].Begin(new Vector2(transform.position.x - 1 + i, transform.position.y));
        }
        transform.position = new Vector3(-80, -80, transform.position.z);
    }
}
