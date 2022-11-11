using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    public string levelName;
    public Vector2 startPosition;
    public BoolValue loadFromTransition;
    [SerializeField] Signal fadeSceneOutSignal;
    public PlayerInfo.EgyptTransitions roomTransition;
 
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            if (!PlayerInfo.Instance.travelledTransitions.ContainsKey(roomTransition)) { 
                PlayerInfo.Instance.travelledTransitions.Add(roomTransition, PlayerInfo.RoomTransitionStates.travelled);
            }else if (PlayerInfo.Instance.travelledTransitions[roomTransition] == PlayerInfo.RoomTransitionStates.known) {
                PlayerInfo.Instance.travelledTransitions[roomTransition] = PlayerInfo.RoomTransitionStates.travelled;
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
        yield return new WaitForSeconds(SceneFader.fadeWait);
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }
}
