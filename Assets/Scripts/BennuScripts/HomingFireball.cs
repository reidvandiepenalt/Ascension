using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingFireball : MonoBehaviour
{
    [SerializeField] MiniHomingFireball[] miniFireballs = new MiniHomingFireball[6];
    [SerializeField] float speed_p1, speed_p2;
    [SerializeField] float time_p1, time_p2;
    [SerializeField] new ParticleSystem particleSystem;
    [SerializeField] Animator anim;
    float currentTime = 0f;
    Transform playerTransform;
    BennuAI.Phase curPhase = BennuAI.Phase.one;
    bool isSpawned = false;


    private void FixedUpdate()
    {
        if(!isSpawned) { return; }

        float angle = Vector2.SignedAngle(transform.position, playerTransform.position);
        transform.position += new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad))
            * ((curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2) * Time.fixedDeltaTime;
        currentTime += Time.fixedDeltaTime;
        if(currentTime >= ((curPhase == BennuAI.Phase.one) ? time_p1 : time_p2))
        {
            anim.SetTrigger("Explode");
            Invoke("End", 1f / 3f);
        }
    }

    public void Begin(Transform player, Vector2 position, BennuAI.Phase phase)
    {
        isSpawned = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        playerTransform = player;
        curPhase = phase;
    }

    void End()
    {
        currentTime = 0;
        isSpawned = false;
        switch (curPhase)
        {
            case BennuAI.Phase.one:
                //spawn 4 minifireballs at position
                for(int i = 0; i < 4; i++)
                {
                    Vector2 targetPos = transform.position + new Vector3(Mathf.Cos((i * 90 + 45) * Mathf.Deg2Rad), Mathf.Sin((i * 90 + 45) * Mathf.Deg2Rad));
                    miniFireballs[i].Begin(playerTransform, new Vector2(transform.position.x, transform.position.y), targetPos, curPhase);
                }
                break;
            case BennuAI.Phase.two:
                //spawn all minifireballs at position
                for(int i = 0; i < miniFireballs.Length; i++)
                {
                    Vector2 targetPos = transform.position + new Vector3(Mathf.Cos((i * 60) * Mathf.Deg2Rad), Mathf.Sin((i * 60) * Mathf.Deg2Rad));
                    miniFireballs[i].Begin(playerTransform, new Vector2(transform.position.x, transform.position.y), targetPos, curPhase);
                }
                break;
        }

        transform.position = new Vector3(-80, -80, transform.position.z);
    }
}
