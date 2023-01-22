using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossAttackSFX : MonoBehaviour
{
    [SerializeField] AudioSource sfx;

    private void OnEnable()
    {
        sfx.Play();
    }

}
