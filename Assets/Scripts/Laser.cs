using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public int damage;
    public Vector2 targetPosition;

    private LineRenderer lr;
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lr.SetPosition(0, transform.position);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, (targetPosition - (Vector2)transform.position).normalized, out hit))
        {
            Debug.Log(hit.collider.gameObject.tag);
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                hit.collider.gameObject.GetComponent<PlayerTestScript>().TakeDamage(damage);
            }
            if (hit.collider)
            {
                
                lr.SetPosition(1, hit.point);
            }
        }
        else lr.SetPosition(1, (targetPosition - (Vector2)transform.position).normalized * 5000);
    }
}
