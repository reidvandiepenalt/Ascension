using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HintText : MonoBehaviour
{
    public Animator animator;
    public TextMeshPro text;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        animator.SetTrigger("FadeIn");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        animator.SetTrigger("FadeOut");
    }
}
