using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    Animator anim;

    [Range(0, 1f)]
    public float distanceToGround;
    public LayerMask groundLayer;
    public Transform LeftLegEffector, RightLegEffector;
    public Transform LeftFootBone, RightFootBone;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!anim.GetBool("Walking"))
        {
            return;
        }

         //left foot
         RaycastHit2D hit = Physics2D.Raycast(LeftLegEffector.position + Vector3.up * distanceToGround, Vector2.down, distanceToGround, groundLayer);
        if (hit)
        {
            Vector2 footPosition = hit.point;
            footPosition.y += distanceToGround;
            LeftLegEffector.transform.position = footPosition;
            //bone rotation is normal to hit surface
            //LeftFootBone.transform.rotation.SetLookRotation(new Vector3(0, 0, hit.transform.rotation.z + 90));
        }

         //right foot
         RaycastHit2D hit2 = Physics2D.Raycast(RightLegEffector.position + Vector3.up, Vector2.down, distanceToGround + 1f, groundLayer);
        if (hit2)
        {
            Vector2 footPosition = hit.point;
            footPosition.y += distanceToGround;
            RightLegEffector.transform.position = footPosition;
            //bone rotation is normal to hit surface
            //RightFootBone.transform.rotation.SetLookRotation(new Vector3(0, 0, hit2.transform.rotation.z + 90));
        }
    }
}
