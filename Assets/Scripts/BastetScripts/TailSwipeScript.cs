using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TailSwipeScript : MonoBehaviour
{
    public int direction;

    [SerializeField] float speed = 20f;
    [SerializeField] float despawnTime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        if(direction < 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        Invoke("Despawn", despawnTime);
    }

    private void FixedUpdate()
    {
        transform.position += Vector3.right * speed * Time.fixedDeltaTime * Mathf.Sign(direction);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Inanimate")){
            Destroy(gameObject);
        }
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
