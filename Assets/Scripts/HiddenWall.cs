using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenWall : MonoBehaviour
{
    [SerializeField] SpriteRenderer rend;

    private void Start()
    {
        if(rend == null) rend = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LeanTween.alpha(gameObject, 0.15f, 0.5f);
        }
    }
}
