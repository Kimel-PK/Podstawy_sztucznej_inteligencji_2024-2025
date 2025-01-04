using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar : MonoBehaviour {
	
	public static List<INode> Find (INode startNode, INode endNode) {
		if (startNode == null) {
			Debug.Log("startNode is null, aborting pathfinding");
			return new List<INode>();
		}
		
		if (endNode == null) {
			Debug.Log("endNode is null, aborting pathfinding");
			return new List<INode>();
		}

		List<INode> path = new();
		List<PathNode> openList = new() { new PathNode(startNode) };
		List<PathNode> closedList = new();
		
		openList[0].GCost = 0;
		openList[0].HCost = CalculateDistance (startNode.Position, endNode.Position);
		
		int steps = 0;
		while (openList.Count > 0 && steps < 1000) {
			PathNode currentNode = FindLowestFCostNode (openList);
			// target found
			if (currentNode.GraphNode == endNode) {
				while (currentNode.PreviousNode != null) {
					path.Insert (0, currentNode.GraphNode);
					currentNode = currentNode.PreviousNode;
				}
				return path;
			}
			
			openList.Remove (currentNode);
			closedList.Add (currentNode);
			
			foreach (INode node in currentNode.GraphNode.Neighbours) {
				PathNode pathNode = new(node);
				if (closedList.Contains (pathNode, new NodeCompare()))
					continue;
				
				float nowyKosztG = currentNode.GCost + CalculateDistance (currentNode.GraphNode.Position, node.Position);
				if (openList.Contains (pathNode))
					pathNode = openList [openList.IndexOf (pathNode)];
				
				if (nowyKosztG < pathNode.GCost) {
					pathNode.PreviousNode = currentNode;
					pathNode.GCost = nowyKosztG;
					pathNode.HCost = CalculateDistance (pathNode.GraphNode.Position, endNode.Position);
					
					if (!openList.Contains (pathNode, new NodeCompare()))
						openList.Add (pathNode);
				}
			}
			
			steps++;
		}
		
		if (steps == 1000)
			Debug.LogWarning ("Pathfinding limit reached");
		
		float lowestHCost = CalculateDistance (startNode.Position, endNode.Position);
		INode closestNode = startNode;
		
		foreach (PathNode pathNode in closedList) {
			if (pathNode.HCost < lowestHCost) {
				lowestHCost = pathNode.HCost;
				closestNode = pathNode.GraphNode;
			}
		}
		
		foreach (PathNode pathNode in openList) {
			if (pathNode.HCost < lowestHCost) {
				lowestHCost = pathNode.HCost;
				closestNode = pathNode.GraphNode;
			}
		}
		
		if (closestNode != startNode) {
			Debug.Log ("Couldn't find direct path, return closest node");
			return Find (startNode, closestNode);
		}
		
		Debug.LogWarning ("Couldn't find a path!");
		return null;
	}
	
	private class NodeCompare : IEqualityComparer<PathNode> {
		public bool Equals(PathNode a, PathNode b) {
			if (a == null && b == null)
				return true;
			if (a == null || b == null)
				return false;
			if (a.GraphNode == b.GraphNode)
				return true;
			return false;
		}

		public int GetHashCode(PathNode a) {
			float hCode = a.GraphNode.Position.x + a.GraphNode.Position.y;
			return hCode.GetHashCode();
		}
	}
	
	private static float CalculateDistance (Vector2 a, Vector2 b) {
		return Mathf.Sqrt ((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));
	}
	
	private static PathNode FindLowestFCostNode (List<PathNode> nodeList) {
		PathNode lowest = nodeList[0];
		for (int i = 1; i < nodeList.Count; i++) {
			if (nodeList[i].FCost < lowest.FCost)
				lowest = nodeList[i];
		}
		return lowest;
	}
	
	private class PathNode {

		public INode GraphNode { get; }
		public PathNode PreviousNode { get; set; }

		public float GCost = float.PositiveInfinity;
		public float HCost;
		public float FCost => GCost + HCost;
		
		public PathNode (INode node) => GraphNode = node;
	}
}
