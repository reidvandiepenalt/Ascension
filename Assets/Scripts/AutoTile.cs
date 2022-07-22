using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTile : MonoBehaviour
{
    [SerializeField] private float multiplierX = 1;
    [SerializeField] private float multiplierY = 1;
    [SerializeField] private int renderQueue = 2000;
    [SerializeField] bool overrideScale = false;
    [SerializeField] Vector2 offset = Vector2.zero;
    Renderer Rend;
    public void OnValidate()
    {
        /*
        //get the renderer and scale the texture to size of transform
        Rend = GetComponent<Renderer>();
        Material tempMaterial = new Material(Rend.sharedMaterial);
        float tileX = transform.localScale.x * multiplierX;
        float tileY = transform.localScale.y * multiplierY;
        tempMaterial.mainTextureScale = new Vector2(tileX, tileY);
        tempMaterial.renderQueue = renderQueue;
        tempMaterial.mainTextureOffset = offset;
        Rend.sharedMaterial = tempMaterial;
        */
    }
}
