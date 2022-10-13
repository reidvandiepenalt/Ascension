using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnscaledTimeParticle : MonoBehaviour
{
    ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        //ignore timescale in animations
        if(Time.timeScale < 1.0f)
        {
            particleSystem.Simulate(Time.unscaledDeltaTime, true, false);
        }
    }
}
