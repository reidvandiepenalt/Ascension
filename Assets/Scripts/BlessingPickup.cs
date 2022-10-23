using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlessingPickup : MonoBehaviour
{
    [SerializeField] Blessing blessing;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            blessing.unlocked = true;

            //anim
            StartCoroutine(nameof(Anim));
        }
        
    }

    protected virtual IEnumerator Anim()
    {
        Destroy(gameObject);
        return null;
    }
}
