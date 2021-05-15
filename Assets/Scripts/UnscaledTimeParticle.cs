using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnscaledTimeParticle : MonoBehaviour
{
    ParticleSystem particleSystem;

    private void Start()
    {
        particleSystem = this.GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale < 1.0f)
        {
            particleSystem.Simulate(Time.unscaledDeltaTime, true, false);
        }
    }
}
