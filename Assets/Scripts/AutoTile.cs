using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTile : MonoBehaviour
{
    [SerializeField] private float tileX = 1;
    [SerializeField] private float tileY = 1;
    Mesh mesh;
    Renderer Rend;
    private Material matCopy;
    public void Awake()
    {
        //get the renderer and scale the texture to size of transform
        Rend = GetComponent<Renderer>();
        matCopy = new Material(Rend.material);
        Rend.material = matCopy;
        mesh = GetComponent<MeshFilter>().mesh;
        tileX = transform.localScale.x;
        tileY = transform.localScale.y;
        Rend.material.mainTextureScale = new Vector2(tileX, tileY);
    }
}
