using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorFire : MonoBehaviour
{
    const float maxTime = 0.5f;
    float currentTime = 0.0f;
    bool isSpawned = false;

    private void FixedUpdate()
    {
        if (isSpawned)
        {
            currentTime += Time.fixedDeltaTime;
            if(currentTime >= maxTime)
            {
                End();
            }
        }
    }

    public void Begin(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
        isSpawned = true;
    }

    void End()
    {
        transform.position = new Vector3(-80, -80, transform.position.z);
        isSpawned = false;
        currentTime = 0f;
    }
}
