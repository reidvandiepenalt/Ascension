using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePlume : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] FirePlumeFireball[] fireballs;

    public void Begin(Vector2 position, BennuAI.Phase phase)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        anim.SetTrigger("Plume");
        
        if(phase == BennuAI.Phase.two)
        {
            Invoke("SpawnFireballs", 0.25f);
        }

        Invoke("End", 35f/60f);
    }

    void SpawnFireballs()
    {
        //spawn fireballs and give force / velocity
    }

    void End()
    {
        transform.position = new Vector3(-80, -80, transform.position.z);
    }
}
