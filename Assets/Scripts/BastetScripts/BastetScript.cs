using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BastetScript : MonoBehaviour
{
    [SerializeField] Vector2[] jumpPoints;
    [SerializeField] GameObject jumpParent;

    [SerializeField] float speed;
    bool facingRight = true;

    State state = State.walk;
    Phase phase = Phase.one;

    enum State
    {
        walk,
        sprint,
        charge,
        clawSwipe,
        tailWhip,
        clawPlatform,
        backflip
    }

    enum Phase
    {
        one,
        two
    }

    private void OnValidate()
    {
        Transform[] ts = jumpParent.GetComponentsInChildren<Transform>();
        if(ts != null)
        {
            jumpPoints = new Vector2[ts.Length];
            for(int i = 0; i < ts.Length; i++)
            {
                jumpPoints[i] = ts[i].position;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    void FixedUpdate()
    {
        if (Pause.isPaused) { return; }


    }
}
