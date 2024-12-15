using System;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
    
    [SerializeField] private LineRenderer lineRenderer;

    [SerializeField] private Transform pointsParent;
    
    private void Start() {
        if (!lineRenderer || pointsParent.childCount == 0)
            return;

        Vector3[] positions = new Vector3[pointsParent.childCount];
        int i = 0;
        foreach (Transform point in pointsParent) {
            positions[i] = point.position;
            i++;
        }

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public bool IsPointInsideObstacle(Vector2 point) {
        return false;
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < pointsParent.childCount; i++) {
            Transform point = pointsParent.GetChild(i);
            Transform nextPoint = pointsParent.GetChild((i + 1) % pointsParent.childCount);
            Debug.DrawLine(point.position, nextPoint.position, Color.white);
        }
    }
}