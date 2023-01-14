using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySFXManager : MonoBehaviour
{
    public AudioSource Movement;
    public AudioSource Jump;
    public AudioSource Land;
    public AudioSource Hit;
    public AudioSource Attack;

    public void PlayMove()
    {
        Movement.Play();
    }

    public void StopMove()
    {
        Movement.Stop();
    }

    public void PlayJump()
    {
        Jump.Play();
    }

    public void PlayLand()
    {
        Land.Play();
    }

    public void PlayHit()
    {
        Hit.Play();
    }

    public void PlayAttack()
    {
        Attack.Play();
    }
}
