 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossItem : MonoBehaviour
{
    [SerializeField] Signal signal;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        SetFlags();

        signal.RaiseSignal();

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
