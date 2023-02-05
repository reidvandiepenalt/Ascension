using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BennuAI;

public abstract class BossAI : MonoBehaviour
{
    [SerializeField] protected GameObject item;
    [SerializeField] protected EnemyHealth healthManager;
    [SerializeField] protected Animator anim;
    protected bool AIisActive = false;
    virtual public void ActivateAI()
    {
        AIisActive = true;
    }

    abstract public void OnStun(object param);

    abstract public void OnHit(object parameter);

    abstract protected void Die();
}
