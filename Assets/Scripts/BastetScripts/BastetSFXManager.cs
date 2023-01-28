using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetSFXManager : MonoBehaviour
{
    public AudioSource Movement;
    public AudioSource Hurt;
    public AudioSource Call;
    public AudioSource TailWhip;
    public AudioSource Jump;
    public AudioSource Swipe;
    public AudioSource EyeFlash;

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
        Call.Play();
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
}
