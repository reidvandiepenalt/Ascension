using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniHomingFireball : MonoBehaviour
{
    [SerializeField] float speed_p1, speed_p2;
    [SerializeField] float time_p1, time_p2;
    [SerializeField] AudioSource sfx;
    float currentTime = 0f;
    Transform playerTransform;
    Vector2 beginTarget;
    BennuAI.Phase curPhase = BennuAI.Phase.one;
    bool isSpawned = false;
    bool reachedBeginTarget = false;
    float Speed { get => ((curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2); }

    private void FixedUpdate()
    {
        if (!isSpawned) { return; }

        currentTime += Time.fixedDeltaTime;
        if (reachedBeginTarget)
        {

            float angle = Mathf.Atan2(playerTransform.position.y - transform.position.y, playerTransform.position.x - transform.position.x);
            transform.position += new Vector3(Mathf.Cos(angle) * Speed * Time.fixedDeltaTime, Mathf.Sin(angle) * Speed * Time.fixedDeltaTime);
        }
        else
        {
            float angle = Mathf.Atan2(beginTarget.y - transform.position.y, beginTarget.x - transform.position.x);
            transform.position += new Vector3(Mathf.Cos(angle) * Speed * Time.fixedDeltaTime, Mathf.Sin(angle) * Speed * Time.fixedDeltaTime);
            if(Vector2.Distance(transform.position, beginTarget) < 0.75f) { reachedBeginTarget = true; }
        }

        if (currentTime >= ((curPhase == BennuAI.Phase.one) ? time_p1 : time_p2))
        {
            End();
        }
    }

    public void Begin(Transform player, Vector2 position, Vector2 firstPosition, BennuAI.Phase phase)
    {
        currentTime = 0;
        isSpawned = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        playerTransform = player;
        beginTarget = firstPosition;
        curPhase = phase;
        sfx.Play();
    }

    void End()
    {
        currentTime = 0;
        isSpawned = false;
        reachedBeginTarget = false;
        transform.position = new Vector3(-80, -80, transform.position.z);
        sfx.Stop();
    }
}
