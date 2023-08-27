 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossItem : MonoBehaviour
{
    [SerializeField] Signal onPickup;
    [SerializeField] GameObject tutorialCanvasPrefab;

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SetFlags();

            onPickup.RaiseSignal();

            //anim
            StartCoroutine(nameof(Anim));
        }
    }

    protected abstract void SetFlags();
    protected virtual IEnumerator Anim()
    {
        Instantiate(tutorialCanvasPrefab);
        Destroy(gameObject);
        return null;
    }
}
