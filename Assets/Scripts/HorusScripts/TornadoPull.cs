using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoPull : MonoBehaviour
{
    Transform player;
    [SerializeField] float pullStrength;
    [SerializeField] AudioSource sfx;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        player.Translate(new Vector2(pullStrength * Time.deltaTime * ((transform.position.x > player.transform.position.x) ? 1 : -1 ), 0));
    }

    private void OnEnable()
    {
        sfx.Play();
    }

    private void OnDisable()
    {
        sfx.Stop();
    }
}
