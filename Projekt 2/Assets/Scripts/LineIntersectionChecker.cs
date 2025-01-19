using UnityEngine;

public class LineIntersectionChecker : MonoBehaviour
{
    /// <summary>
    /// Checks if the line between two points intersects with any edges of obstacles.
    /// </summary>
    /// <param name="myPosition">Start of the line (player position).</param>
    /// <param name="enemyPosition">End of the line (enemy position).</param>
    /// <returns>True if the line intersects with any edges, otherwise false.</returns>
    public static bool IsLineBlocked(Vector3 myPosition, Vector3 enemyPosition)
    {
        foreach (Obstacle obstacle in Obstacle.All)
        {
            foreach (Edge edge in obstacle.edges)
            {
                if (DoesLineIntersect(myPosition, enemyPosition, edge.Start, edge.End))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if two line segments intersect.
    /// </summary>
    /// <param name="p1">Start of the first line.</param>
    /// <param name="p2">End of the first line.</param>
    /// <param name="q1">Start of the second line (edge start).</param>
    /// <param name="q2">End of the second line (edge end).</param>
    /// <returns>True if the lines intersect, otherwise false.</returns>
    private static bool DoesLineIntersect(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
    {
        // Use X and Y components only
        Vector2 A1 = new(p1.x, p1.y);
        Vector2 A2 = new(p2.x, p2.y);
        Vector2 B1 = new(q1.x, q1.y);
        Vector2 B2 = new(q2.x, q2.y);

        // Calculate the determinant of the directions
        Vector2 r = A2 - A1;
        Vector2 s = B2 - B1;
        float rxs = Cross(r, s);
        float t = Cross(B1 - A1, s) / rxs;
        float u = Cross(B1 - A1, r) / rxs;

        // If rxs is zero, lines are parallel or collinear (no intersection)
        if (Mathf.Approximately(rxs, 0))
        {
            return false;
        }

        // Lines intersect if t and u are between 0 and 1
        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }

    /// <summary>
    /// Computes the 2D cross product of two vectors.
    /// </summary>
    /// <param name="v1">First vector.</param>
    /// <param name="v2">Second vector.</param>
    /// <returns>Scalar cross product.</returns>
    private static float Cross(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.y - v1.y * v2.x;
    }
    
    /// <summary>
    /// Checks if the direction vector is within the given angular margin towards the target position.
    /// </summary>
    /// <param name="position">The position to check towards.</param>
    /// <param name="direction">The current direction vector.</param>
    /// <param name="marginDegrees">The allowed angular margin in degrees.</param>
    /// <returns>True if within the margin; otherwise, false.</returns>
    public static bool IsVectorRotatedTowards(Vector3 position, Vector3 direction, float marginDegrees)
    {
        // Normalize the vectors
        Vector3 normalizedPosition = position.normalized;
        Vector3 normalizedDirection = direction.normalized;

        // Calculate the angle between the vectors
        float angle = Vector3.Angle(normalizedDirection, normalizedPosition);

        // Check if the angle is within the margin
        return angle <= marginDegrees;
    }
}