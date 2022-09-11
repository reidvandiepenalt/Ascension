using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    public string levelName;
    public Vector2 startPosition;
    public BoolValue loadFromTransition;
    public float fadeWait;
    [SerializeField] Signal fadeSceneOutSignal;
    public PlayerInfo.EgyptTransitions roomTransition;
 
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            if (!PlayerInfo.Instance.travelledTransitions.Contains(roomTransition)) { 
                PlayerInfo.Instance.travelledTransitions.Add(roomTransition);
            }
            fadeSceneOutSignal.RaiseSignal();
            LoadScene();
        }
    }

    public void LoadScene()
    {
        loadFromTransition.Value = true;
        PlayerInfo.Instance.loadPos = startPosition;
        StartCoroutine(FadeCo());
    }

    public IEnumerator FadeCo()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(levelName);
        yield return new WaitForSeconds(fadeWait);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
