using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Cutscene : MonoBehaviour
{
    public VideoPlayer video;
    public ChangeScenes changeScenes;


    // Start is called before the first frame update
    void Start()
    {
        Invoke("ChangeScenes", (float)video.clip.length + 1);
    }

    void ChangeScenes()
    {
        changeScenes.LoadScene();
    }
}
