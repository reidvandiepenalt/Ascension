using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaProjectileScript : MonoBehaviour
{
    public bool DestroyOnWall;
    public Vector2 targetPosition;
    public float speed;
    public bool destroy = false;

    /// <summary>
    /// Moves along a linear path until the projectile hits a wall
    /// </summary>
    /// <returns></returns>
    public IEnumerator DirectedProj()
    {
        while (!destroy)
        {
            Vector3 direction = (targetPosition - (Vector2)transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Destroys the projectile if it hits the wall and is a directed proj
    /// </summary>
    /// <param name="collision"></param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Inanimate") && DestroyOnWall)
        {
            destroy = true;
        }
    }
}
