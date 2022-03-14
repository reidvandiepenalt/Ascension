using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyJumpPath : MonoBehaviour
{
    /// <summary>
    /// Generates a jumping path to move through
    /// </summary>
    /// <param name="endPoint">End point of the jump</param>
    void GeneratePath(Vector2 endPoint, float timeToMove, float gravity, ref Queue<Vector2> path)
    {
        GeneratePath(transform.position, endPoint, timeToMove, gravity, ref path);
    }

    /// <summary>
    /// Generates a path from start to end point
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    void GeneratePath(Vector2 startPoint, Vector2 endPoint, float timeToMove, float gravity, ref Queue<Vector2> path)
    {
        endPoint.y += 0.1f;
        Debug.DrawLine(startPoint, endPoint, Color.red, 5f);

        int numSteps = (int)((1 / Time.fixedDeltaTime) * timeToMove);
        float stepDist = (endPoint.x - startPoint.x) / numSteps;

        float initVy = ((endPoint.y - startPoint.y) / timeToMove) + (-gravity / 2 * timeToMove);
        for (int i = 1; i <= numSteps; i++)
        {
            path.Enqueue(new Vector2(startPoint.x + i * stepDist,
                (gravity / 2) * Mathf.Pow(i * Time.fixedDeltaTime, 2) + (i * Time.fixedDeltaTime * initVy) + startPoint.y));
        }
    }
}
