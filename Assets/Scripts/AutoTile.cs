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
    Mesh mesh;
    Renderer Rend;
    private Material matCopy;
    public void OnValidate()
    {
        //get the renderer and scale the texture to size of transform
        Rend = GetComponent<Renderer>();
        matCopy = new Material(Rend.material);
        Rend.sharedMaterial = matCopy;
        mesh = GetComponent<MeshFilter>().mesh;
        float tileX = transform.localScale.x * multiplierX;
        float tileY = transform.localScale.y * multiplierY;
        Rend.material.mainTextureScale = new Vector2(tileX, tileY);
        Rend.material.renderQueue = renderQueue;
        Rend.material.mainTextureOffset = offset;
    }
}
