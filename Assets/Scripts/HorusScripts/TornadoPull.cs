using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoPull : MonoBehaviour
{
    Transform player;
    [SerializeField] float pullStrength;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        player.Translate(new Vector2(pullStrength * Time.deltaTime * ((transform.position.x > player.transform.position.x) ? 1 : -1 ), 0));
    }
}
