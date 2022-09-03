using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScenes : MonoBehaviour
{
    public string levelName;
    public Vector2 startPosition;
    public VectorValue positionStorage;
    public BoolValue loadFromTransition;
    public GameObject fadeInPanel;
    public GameObject fadeOutPanel;
    public float fadeWait;
    [SerializeField] Signal fadeSceneOutSignal;

    private void Awake()
    {
        //fade the screen in
        if (fadeInPanel != null)
        {
            GameObject panel = Instantiate(fadeInPanel, Vector3.zero, Quaternion.identity);
            Destroy(panel, fadeWait);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !collision.isTrigger)
        {
            fadeSceneOutSignal.RaiseSignal();
            LoadScene();
        }
    }

    public void LoadScene()
    {
        loadFromTransition.Value = true;
        positionStorage.storedValue = startPosition;
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
