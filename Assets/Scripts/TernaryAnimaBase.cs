using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TernaryAnimaBase : MonoBehaviour
{
    Transform parentTransform;
    bool flipped = false;
    float fullRotationTime = 1.5f;
    float rotationTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        parentTransform = GameObject.FindWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (parentTransform)
        {
            if (parentTransform.localScale.x < 0 && !flipped)
            {
                transform.eulerAngles = new Vector3(0, 180, transform.eulerAngles.z);
                flipped = true;
            }else if (parentTransform.localScale.x > 0 && flipped)
            {
                transform.eulerAngles = new Vector3(0, 0, transform.eulerAngles.z);
                flipped = false;
            }
        }

        rotationTimer += Time.deltaTime;
        if (rotationTimer > fullRotationTime) rotationTimer -= fullRotationTime;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, Mathf.Lerp(0, 360, rotationTimer / fullRotationTime) * (flipped ? -1 : 1));
    }
}
