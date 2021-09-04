using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusFeatherScript : MonoBehaviour
{
    public float speed;
    public Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Inanimate"))
        {
            gameObject.SetActive(false);
        }
    }

    //Object Recycling
    private void OnDisable()
    {
        transform.position = new Vector3(-100, -100, transform.position.z);
    }

    public void Reset(Vector2 startPos, Vector2 dir)
    {
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        direction = dir;
        transform.rotation = Quaternion.identity;
        transform.Rotate(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + ((Mathf.Sign(dir.x) == -1) ? 180 : 0));
    }
}
