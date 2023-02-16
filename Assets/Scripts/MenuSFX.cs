using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuSFX : MonoBehaviour
{
    [SerializeField] AudioSource sfx;

    private void OnValidate()
    {
        if(sfx == null) sfx = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        sfx.ignoreListenerPause = true;
    }
}
