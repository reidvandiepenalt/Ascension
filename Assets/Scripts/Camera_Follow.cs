using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    public Controller2D target;

    FocusArea focusArea;

    public float verticalOffset;


    
    public float lookAheadDistX;
    public float lookSmoothTimeX;
    public float lookAheadDistY;
    public float lookSmoothTimeY;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirX;
    float smoothLookVelocityX;
    float smoothLookVelocityY;
    float currentLookAheadY;
    float targetLookAheadY;
    float lookAheadDirY;

    bool lookAheadStopped;

    private void Start()
    {
        focusArea = new FocusArea(target.collider.bounds);
    }

    private void LateUpdate()
    {
        focusArea.Update(target.collider.bounds);

        Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;

        if (focusArea.velocity.x != 0)
        {
            lookAheadDirX = Mathf.Sign(focusArea.velocity.x);
            if (Input.GetAxisRaw("Horizontal") == 0)
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDistX - currentLookAheadX) / 4f;
                }
            }
            else
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirX * lookAheadDistX;
            }
        }
        if (focusArea.velocity.y != 0)
        {
            lookAheadDirY = Mathf.Sign(focusArea.velocity.y);
            if (lookAheadDirY == -1)
            {
                targetLookAheadY = lookAheadDirY * lookAheadDistY * 1.5f;
            }
            else
            {
                targetLookAheadY = lookAheadDirY * lookAheadDistY;
            }
            
        } else
        {
            targetLookAheadY = 0;
        }


        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
        currentLookAheadY = Mathf.SmoothDamp(currentLookAheadY, targetLookAheadY, ref smoothLookVelocityY, lookSmoothTimeY);

        focusPosition += Vector2.up * currentLookAheadY;
        focusPosition += Vector2.right * currentLookAheadX;

        transform.position = (Vector3)focusPosition + Vector3.forward * -10;
    }

    struct FocusArea
    {
        public Vector2 center, velocity;
        float left, right, top, bottom;

        public FocusArea(Bounds targetBounds)
        {
            left = targetBounds.min.x;
            right = targetBounds.max.x;
            bottom = targetBounds.min.y;
            top = targetBounds.max.y;

            velocity = Vector2.zero;
            center = new Vector2(targetBounds.center.x, targetBounds.center.y);
        }

        public void Update(Bounds targetBounds)
        {
            float shiftX = 0;
            if(targetBounds.min.x < left)
            {
                shiftX = targetBounds.min.x - left;
            } else if (targetBounds.max.x > right)
            {
                shiftX = targetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (targetBounds.min.y < bottom)
            {
                shiftY = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                shiftY = targetBounds.max.y - top;
            }
            bottom += shiftY;
            top += shiftY;

            center = new Vector2(targetBounds.center.x, targetBounds.center.y);
            velocity = new Vector2(shiftX, (Mathf.Abs(shiftY)>0.1)?shiftY:0);
        }
    }


}
