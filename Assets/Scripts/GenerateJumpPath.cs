using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateJumpPath : MonoBehaviour
{
    /// <summary>
    /// Generates a path from start to end point
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    public Queue<Vector2> GeneratePath(Vector2 startPoint, Vector2 endPoint, float timeToMove, float gravity)
    {
        Queue<Vector2> path = new Queue<Vector2>();

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

        return path;
    }
}
