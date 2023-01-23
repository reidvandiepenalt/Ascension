using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorusSFXManager : MonoBehaviour
{
    [SerializeField] AudioSource dive;
    [SerializeField] AudioSource call;
    [SerializeField] AudioSource wings;
    [SerializeField] AudioSource swoop;
    [SerializeField] AudioSource hurt;

    public void PlayDive()
    {
        StopWings();
        dive.Play();
    }

    public void StopDive()
    {
        dive.Stop();
    }

    public void PlayCall()
    {
        call.Play();
    }

    public void PlayWings()
    {
        if (dive.isPlaying || swoop.isPlaying || wings.isPlaying) return;
        wings.Play();
    }

    public void StopWings()
    {
        wings.Stop();
    }

    public void PlaySwoop()
    {
        StopWings();
        swoop.Play();
    }

    public void StopSwoop()
    {
        swoop.Stop();
    }

    public void PlayHurt()
    {
        hurt.Play();
    }
}
