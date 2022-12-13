using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFXManager : MonoBehaviour
{
    public AudioSource Melee1;
    public AudioSource Melee2;
    public AudioSource Walk;
    public AudioSource Jump;
    public AudioSource DoubleJump;
    public AudioSource Glide;
    public AudioSource Land;

    public void PlayMelee1()
    {
        Melee1.Play();
    }

    public void PlayMelee2()
    {
        Melee2.Play();
    }

    public void PlayWalk()
    {
        Walk.Play();
    }

    public void StopWalk()
    {
        Walk.Stop();
    }

    public void PlayJump()
    {
        Jump.Play();
    }

    public void PlayDoubleJump()
    {
        DoubleJump.Play();
    }

    public void PlayGlide()
    {
        Glide.Play();
    }

    public void PlayLand()
    {
        Land.Play();
    }
}
