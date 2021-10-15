using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScript : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Camera camera;
    [SerializeField] bool camBased = false;

    float leftBound;
    float rightBound;

    float camWidth;
    float spriteWidth;

    // Start is called before the first frame update
    void Start()
    {
        if (camBased)
        {
            camWidth = camera.orthographicSize;
            leftBound = -camWidth;
            rightBound = camWidth;
        }
        
        spriteWidth = gameObject.GetComponent<SpriteRenderer>().bounds.extents.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(moveSpeed * Time.fixedDeltaTime, 0, 0);
        if(transform.position.x - spriteWidth > rightBound)
        {
            if (!camBased) { Destroy(gameObject); return; }
            transform.position = new Vector3(leftBound - spriteWidth, transform.position.y, transform.position.z);
        }else if (transform.position.x + spriteWidth < leftBound)
        {
            if (!camBased) { Destroy(gameObject); return; }
            transform.position = new Vector3(rightBound + spriteWidth, transform.position.y, transform.position.z);
        }
    }


    public void SetConditions(float left, float right, float speed)
    {
        leftBound = left;
        rightBound = right;
        moveSpeed = speed;
    }
}
