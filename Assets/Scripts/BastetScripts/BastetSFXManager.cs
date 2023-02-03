using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetSFXManager : MonoBehaviour
{
    public AudioSource Movement;
    public AudioSource Hurt;
    public AudioSource Call1;
    public AudioSource Call2;
    public AudioSource TailWhip;
    public AudioSource Jump;
    public AudioSource Swipe;
    public AudioSource EyeFlash;
    public AudioSource Stomp;

    System.Random rng;

    public void Start()
    {
        rng = new System.Random();
    }

    public void PlayMovement()
    {
        if (Movement.isPlaying) return;
        Movement.Play();
    }

    public void StopMovement()
    {
        Movement.Stop();
    }

    public void PlayHurt()
    {
        Hurt.Play();
    }

    public void PlayCall()
    {
        if(rng.NextDouble() > 0.5f)
        {
            Call1.Play();
        }
        else
        {
            Call2.Play();
        }
    }

    public void PlayTailWhip()
    {
        TailWhip.Play();
    }

    public void PlayJump()
    {
        Jump.Play();
    }

    public void PlaySwipe()
    {
        Swipe.Play();
    }

    public void PlayEyeFlash()
    {
        EyeFlash.Play();
    }

    public void PlayStomp()
    {
        Stomp.Play();
    }
}
