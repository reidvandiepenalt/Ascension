using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightbeamIdle : MonoBehaviour
{
    [SerializeField] MeshRenderer rend;
    [SerializeField] float offsetRate;

    void FixedUpdate()
    {
        Debug.Log(rend.sharedMaterial.mainTextureOffset);
        rend.sharedMaterial.mainTextureOffset = rend.sharedMaterial.mainTextureOffset + (Vector2.up * (offsetRate * Time.fixedDeltaTime));
        rend.material.mainTextureOffset = rend.material.mainTextureOffset + (Vector2.up * (offsetRate * Time.fixedDeltaTime));

    }
}
