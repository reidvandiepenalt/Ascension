using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupTutorial : MonoBehaviour
{
    [SerializeField] float timeTilClearable = 1;
    float passedTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        Pause.pauseState = Pause.PauseState.Pickup;
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        passedTime += Time.unscaledDeltaTime;
        if (Input.anyKeyDown && passedTime > timeTilClearable)
        {
            Time.timeScale = 1;
            Pause.pauseState = Pause.PauseState.Playing;
            Destroy(gameObject);
        }
    }
}
