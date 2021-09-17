using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusWingAttackScript : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float timeToDespawn;
    public Vector2 moveDirection;

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += (Vector3)(speed * moveDirection * Time.fixedDeltaTime);
    }

    public void Reset(Vector2 position, Vector2 direction)
    {
        gameObject.SetActive(true);
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        moveDirection = direction;
        transform.rotation = Quaternion.Euler(0 , 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg));
        Invoke("Stop", timeToDespawn);
    }

    void Stop()
    {
        gameObject.SetActive(false);
    }
}
