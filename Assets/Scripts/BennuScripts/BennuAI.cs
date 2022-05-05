using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BennuAI : MonoBehaviour
{
    [SerializeField] EnemyCollisionMovementHandler movement;
    [SerializeField] EnemyHealth healthManager;
    [SerializeField] Animator anim;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float speed;
    bool facingRight = true;
    float gravity = -80;//same as player

    Attack attack;
    State state = State.flying;
    Phase phase = Phase.one;


    [SerializeField] Transform playerTransform;
    Collider2D playerCollider;
    float playerGroundOffset;


    enum Attack {
        plume,
        homingBomb,
        fireArc,
        fireBeam,
        diveLand,
        landedBeam
    }

    enum State
    {
        flying,
        landed
    }

    enum Phase
    {
        one,
        two,
        three
    }



    // Start is called before the first frame update
    void Start()
    {
        groundLayer = LayerMask.GetMask("Ground");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerCollider = playerTransform.gameObject.GetComponent<Collider2D>();
        playerGroundOffset = playerCollider.bounds.extents.y;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
