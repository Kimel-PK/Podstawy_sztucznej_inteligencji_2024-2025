using System;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    
    public static List<Obstacle> All = new ();
    
    public List<Edge> edges = new();
    
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform pointsParent;

    private void Awake() {
        All.Add(this);
    }

    private void Start() {
        if (!lineRenderer || pointsParent.childCount == 0)
            return;

        Vector3[] positions = new Vector3[pointsParent.childCount];
        int i = 0;
        foreach (Transform point in pointsParent) {
            positions[i] = point.position;
            i++;
        }

        for (i = 0; i < pointsParent.childCount; i++) {
            Transform point = pointsParent.GetChild(i);
            Transform nextPoint = pointsParent.GetChild((i + 1) % pointsParent.childCount);
            
            edges.Add(new Edge {
                Start = point.position,
                End = nextPoint.position
            });
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
    
    public static bool IsCircleTouchingAnyObstacle(Vector2 point, float agentRadius) {
        foreach (Obstacle obstacle in All) {
            if (obstacle.IsCircleTouchingObstacle(point, agentRadius))
                return true;
        }
        return false;
    }
    
    public bool IsCircleTouchingObstacle(Vector2 point, float radius) {
        foreach (Edge edge in edges) {
            // Vector from start to end of the segment
            Vector2 segment = edge.End - edge.Start;

            // Vector from start of the segment to the point
            Vector2 pointToStart = point - (Vector2)edge.Start;

            // Compute the squared length of the segment
            float segmentLengthSquared = segment.sqrMagnitude;

            // Handle the edge case where the segment is a point
            if (segmentLengthSquared == 0)
                return Vector2.Distance(point, edge.Start) < radius;

            // Project point onto the segment (using dot product)
            float t = Vector2.Dot(pointToStart, segment) / segmentLengthSquared;

            // Clamp the projection factor t between 0 and 1
            t = Math.Clamp(t, 0, 1);

            // Find the closest point on the segment
            Vector2 closestPoint = (Vector2)edge.Start + t * segment;

            // Return the distance from the point to the closest point on the segment
            if (Vector2.Distance(point, closestPoint) < radius)
                return true;
        }

        return false;
    }
    
    public static bool IsPointInsideAnyObstacle(Vector2 point) {
        foreach (Obstacle obstacle in All) {
            if (obstacle.IsPointInsideObstacle(point))
                return true;
        }
        return false;
    }

    public bool IsPointInsideObstacle(Vector2 point) {
        int count = 0;

        foreach (Edge edge in edges)
        {
            (double x1, double y1) = (edge.Start.x, edge.Start.y);
            (double x2, double y2) = (edge.End.x, edge.End.y);

            // Ensure y1 < y2 for simplicity
            if (y1 > y2)
            {
                (x1, x2) = (x2, x1);
                (y1, y2) = (y2, y1);
            }

            // Check if the ray intersects the edge
            if (point.x >= Math.Min(x1, x2) && point.x <= Math.Max(x1, x2) && Math.Abs(point.x - x2) > .00001f) // Avoid double counting at vertices
            {
                // Compute intersection point
                double t = (point.x - x1) / (x2 - x1);
                double intersectionY = y1 + t * (y2 - y1);

                if (intersectionY > point.y) // Ray points upwards
                    count++;
            }
        }
        
        return count % 2 == 1;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < pointsParent.childCount; i++) {
            Transform point = pointsParent.GetChild(i);
            Transform nextPoint = pointsParent.GetChild((i + 1) % pointsParent.childCount);
            Debug.DrawLine(point.position, nextPoint.position, Color.white);
        }
    }
}