using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightScript : MonoBehaviour
{
    /// <summary>
    /// Causes a highlighted object to fade between given alpha values
    /// </summary>
    /// <param name="startAlpha">starting alpha</param>
    /// <param name="endAlpha">ending alpha</param>
    /// <param name="duration">duration of the fade animation</param>
    /// <param name="destroyOnComplete">Whether to destroy the object when the fade animation finishes</param>
    public IEnumerator Fade(float startAlpha, float endAlpha, float duration, bool destroyOnComplete) {
        Image image = gameObject.GetComponent<Image>();
        float timer = 0f;
        while(timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            Color c = image.color;
            c.a += Time.unscaledDeltaTime / duration * (endAlpha - startAlpha);
            image.color = c;
            yield return null;
        }
        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
    }
}
