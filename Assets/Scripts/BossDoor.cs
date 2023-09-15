using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    bool open = true;
    [SerializeField] Collider2D collision;

    public void Close()
    {
        if (open)
        {
            open = false;
            collision.enabled = true;
            LeanTween.moveLocalY(gameObject, transform.position.y - 10, 1);
        }
    }

    public void Open()
    {
        if (!open)
        {
            open = true;
            collision.enabled = false;
            LeanTween.moveLocalY(gameObject, transform.position.y + 10, 1);
        }
    }
}
