using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniHomingFireball : MonoBehaviour
{
    [SerializeField] float speed_p1, speed_p2;
    [SerializeField] float time_p1, time_p2;
    float currentTime = 0f;
    Transform playerTransform;
    Vector2 beginTarget;
    BennuAI.Phase curPhase = BennuAI.Phase.one;
    bool isSpawned = false;
    bool reachedBeginTarget = false;


    private void FixedUpdate()
    {
        if (!isSpawned) { return; }

        if (reachedBeginTarget)
        {

            float angle = Vector2.SignedAngle(transform.position, playerTransform.position);
            transform.position += new Vector3((curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2 * Time.fixedDeltaTime * Mathf.Cos(angle * Mathf.Deg2Rad),
                (curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2 * Time.fixedDeltaTime * Mathf.Sin(angle * Mathf.Deg2Rad));
            currentTime += Time.fixedDeltaTime;
        }
        else
        {
            float angle = Vector2.SignedAngle(transform.position, beginTarget);
            transform.position += new Vector3((curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2 * Time.fixedDeltaTime * Mathf.Cos(angle * Mathf.Deg2Rad),
                (curPhase == BennuAI.Phase.one) ? speed_p1 : speed_p2 * Time.fixedDeltaTime * Mathf.Sin(angle * Mathf.Deg2Rad));
            currentTime += Time.fixedDeltaTime;
        }

        if (currentTime >= ((curPhase == BennuAI.Phase.one) ? time_p1 : time_p2))
        {
            End();
        }
    }

    public void Begin(Transform player, Vector2 position, Vector2 firstPosition, BennuAI.Phase phase)
    {
        isSpawned = true;
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        playerTransform = player;
        beginTarget = firstPosition;
        curPhase = phase;
    }

    void End()
    {
        isSpawned = false;
        reachedBeginTarget = false;
        transform.position = new Vector3(-80, -80, transform.position.z);
    }
}
