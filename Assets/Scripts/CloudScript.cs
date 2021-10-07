using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudScript : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Camera camera;

    float camWidth;
    float spriteWidth;

    // Start is called before the first frame update
    void Start()
    {
        camWidth = camera.orthographicSize;
        spriteWidth = gameObject.GetComponent<SpriteRenderer>().bounds.extents.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Translate(moveSpeed * Time.fixedDeltaTime, 0, 0);
        if(transform.position.x - spriteWidth > camWidth)
        {
            transform.position = new Vector3(-camWidth - spriteWidth, transform.position.y, 0);
        }else if (transform.position.x + spriteWidth < -camWidth)
        {
            transform.position = new Vector3(camWidth + spriteWidth, transform.position.y, 0);
        }
    }
}
