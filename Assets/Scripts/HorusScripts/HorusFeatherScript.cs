using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusFeatherScript : MonoBehaviour
{
    public float speed;
    public Vector2 direction;

    public HorusAI horusAI;

    public bool bounce = false;

    [SerializeField] LayerMask groundLayer;

    private void FixedUpdate()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void Update()
    {
        if (bounce)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1, groundLayer);
            if (hit)
            {
                if (hit.collider.CompareTag("Inanimate"))
                {
                    direction = Vector2.Reflect(direction, hit.normal);
                    bounce = false;
                    transform.rotation = Quaternion.identity;
                    transform.Rotate(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.CompareTag("Inanimate") || collision.CompareTag("NonDestroyingGround")) && !bounce)
        {
            gameObject.SetActive(false);
        }
    }

    //Object Recycling
    private void OnDisable()
    {
        transform.position = new Vector3(-100, -100, transform.position.z);
        horusAI.disabledFeathers.Add(this);
        horusAI.enabledFeathers.RemoveAt(0);
        CancelInvoke();
    }

    public void Reset(Vector2 startPos, Vector2 dir, bool doBounce)
    {
        transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);
        direction = dir;
        transform.rotation = Quaternion.identity;
        transform.Rotate(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
        bounce = doBounce;
        Invoke("OnDisable", 10f);
    }
}
