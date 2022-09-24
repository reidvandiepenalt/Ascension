using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoomTransition : MonoBehaviour
{
    [SerializeField] BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        if(boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    }

    public void EnableCollider()
    {
        boxCollider.enabled = true;
    }
}
