using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
    
    private NavGraph.NavGraphNode target;
    private List<INode> path = new();

    private void OnEnable() {
        NavGraph.OnNavGraphBuild += NavGraph_OnNavGraphBuild;
    }

    private void OnDisable() {
        NavGraph.OnNavGraphBuild -= NavGraph_OnNavGraphBuild;
    }

    private void NavGraph_OnNavGraphBuild() {
        Debug.Log($"agent {name} find random target");
        NavGraph.NavGraphNode startingNode = NavGraph.Instance.GetClosestNode(transform.position);
        NavGraph.NavGraphNode endNode = NavGraph.Instance.GetRandomNode();
        path = AStar.Find(startingNode, endNode);
    }

    private void OnDrawGizmos() {
        for (int i = 0; i < path.Count - 1; i++)
            Debug.DrawLine(path[i].Position / 1000f, path[i + 1].Position / 1000f, Color.red);
    }
}
