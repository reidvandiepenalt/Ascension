using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossItem : MonoBehaviour
{
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        SetFlags();

        //anim
        StartCoroutine("Anim");
    }

    protected abstract void SetFlags();
    protected virtual IEnumerator Anim()
    {
        Destroy(gameObject);
        return null;
    }
}
