using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BennuSFXManager : MonoBehaviour
{
    public AudioSource Hit;
    public AudioSource Movement;
    public AudioSource Call;
    public AudioSource Firebreath;

    public void PlayHit()
    {
        Hit.Play();
    }

    public void PlayCall()
    {
        Call.Play();
    }

    public void StartMovement()
    {
        Movement.Play();
    }

    public void EndMovement()
    {
        Movement.Stop();
    }

    public void StartFirebreath()
    {
        Firebreath.Play();
    }

    public void EndFirebreath()
    {
        Firebreath.Stop();
    }
}
