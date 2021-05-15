using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightScript : MonoBehaviour
{

    public IEnumerator Fade(float start, float end, float duration, bool destroyOnComplete) {
        Image image = gameObject.GetComponent<Image>();
        float timer = 0f;
        while(timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            Color c = image.color;
            c.a += Time.unscaledDeltaTime / duration * (end - start);
            image.color = c;
            yield return null;
        }
        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }
}
