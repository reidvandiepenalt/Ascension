using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraLoad : MonoBehaviour
{
    [SerializeField] CinemachineConfiner confiner;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        confiner.m_BoundingShape2D = GameObject.FindGameObjectWithTag("Room").GetComponent<PolygonCollider2D>();
    }
}
