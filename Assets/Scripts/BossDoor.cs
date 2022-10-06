using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDoor : MonoBehaviour
{
    bool open = true;

    public void Close()
    {
        if (open)
        {
            open = false;
            LeanTween.moveLocalY(gameObject, transform.position.y - 10, 1);
        }
    }

    public void Open()
    {
        if (!open)
        {
            open = true;
            LeanTween.moveLocalY(gameObject, transform.position.y + 10, 1);
        }
    }
}
