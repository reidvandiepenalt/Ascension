using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraLoad : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera cam;

    // Start is called before the first frame update
    void Start()
    {
        if(cam.Follow == null)
        {
            Transform followTarget = GameObject.FindGameObjectWithTag("CamFollowTarget").transform;
            cam.Follow = followTarget;
            cam.LookAt = followTarget;
        }
    }
}
