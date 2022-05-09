using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingFireball : MonoBehaviour
{
    [SerializeField] MiniHomingFireball[] miniFireballs = new MiniHomingFireball[6];
    [SerializeField] float speed_p1, speed_p2;
    [SerializeField] float time_p1, time_p2;
    float currentTime = 0f;
    Transform playerTransform;
    int curPhase = 1;
    bool isSpawned = false;


    private void FixedUpdate()
    {
        if(!isSpawned) { return; }

        float angle = Vector2.SignedAngle(transform.position, playerTransform.position);
        transform.position += new Vector3((curPhase == 1)?speed_p1:speed_p2 * Time.fixedDeltaTime * Mathf.Cos(angle * Mathf.Deg2Rad),
            (curPhase == 1) ? speed_p1 : speed_p2 * Time.fixedDeltaTime * Mathf.Sin(angle * Mathf.Deg2Rad));
        currentTime += Time.fixedDeltaTime;
        if(currentTime >= ((curPhase == 1) ? time_p1 : time_p2))
        {
            End();
        }
    }

    public void Begin(Transform player, Vector2 position, int phase)
    {
        isSpawned = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        playerTransform = player;
        curPhase = phase;
    }

    void End()
    {
        isSpawned = false;
        switch (curPhase)
        {
            case 1:
                //spawn 4 minifireballs at position
                for(int i = 0; i < 4; i++)
                {
                    Vector2 targetPos = transform.position + new Vector3(Mathf.Cos((i * 90 + 45) * Mathf.Deg2Rad), Mathf.Sin((i * 90 + 45) * Mathf.Deg2Rad));
                    miniFireballs[i].Begin(playerTransform, transform.position, targetPos, curPhase);
                }
                break;
            case 2:
                //spawn all minifireballs at position
                for(int i = 0; i < miniFireballs.Length; i++)
                {
                    Vector2 targetPos = transform.position + new Vector3(Mathf.Cos((i * 60) * Mathf.Deg2Rad), Mathf.Sin((i * 60) * Mathf.Deg2Rad));
                    miniFireballs[i].Begin(playerTransform, transform.position, targetPos, curPhase);
                }
                break;
        }
        transform.position = new Vector3(-80, -80, transform.position.z);
    }
}
