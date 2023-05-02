using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaSFXManager : MonoBehaviour
{
    public AudioSource move;

    public void PlayMove()
    {
        move.Play();
    }

    public void StopMove()
    {
        move.Stop();
    }
}
