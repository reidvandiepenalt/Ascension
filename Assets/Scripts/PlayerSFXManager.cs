using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSFXManager : MonoBehaviour
{
    public AudioSource Melee1;
    public AudioSource Melee2;
    public AudioSource Walk;
    public AudioSource Jump;
    public AudioSource DoubleJump;
    public AudioSource Glide;
    public AudioSource Land;
    public AudioSource FeatherShot;
    public AudioSource Hit;
    public AudioSource Guard;
    public AudioSource GuardHit;

    [Space(10)]
    [Header("Defaults")]
    public AudioClip defaultFsSfx;
    public AudioClip defaultGuardHitSfx;


    public void SetFeatherShotSFX(AudioClip sfx)
    {
        if(sfx == null) FeatherShot.clip = defaultFsSfx;
        else FeatherShot.clip = sfx;
    }

    public void SetGuardHitSFX(AudioClip sfx)
    {
        if (sfx == null) GuardHit.clip = defaultGuardHitSfx;
        else GuardHit.clip = sfx;
    }

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

    public void StopGlide()
    {
        Glide.Stop();
    }

    public void PlayLand()
    {
        Land.Play();
    }

    public void PlayFeatherShot()
    {
        FeatherShot.Play();
    }

    public void PlayHit()
    {
        Hit.Play();
    }

    public void PlayGuard()
    {
        Guard.Play();
    }

    public void PlayGuardHit()
    {
        GuardHit.Play();
    }
}
