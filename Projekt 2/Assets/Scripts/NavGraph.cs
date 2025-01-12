using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class NavGraph : MonoBehaviour {

	public static NavGraph Instance { get; private set; }
	
	public static Action OnNavGraphBuild = delegate { };

	[SerializeField] private Vector2 startingPoint;
	[SerializeField] private float gridWidth;
	[SerializeField] private float agentRadius;
	[SerializeField] private float graphPrecision = 0.001f;
	
	[SerializeField] private float topBorder;
	[SerializeField] private float rightBorder;
	[SerializeField] private float bottomBorder;
	[SerializeField] private float leftBorder;

	[SerializeField] private bool animateGraphBuild;
	[SerializeField] private float animationStepTime;
	
	private readonly Dictionary<Vector2Int, NavGraphNode> graphNodes = new();
	
	// exposed because of gizmo animation
	private HashSet<Vector2Int> pointsToConsider = new();
	
	private readonly Vector2Int[] offsets = {
		new(0, 1),   // Up
		new(1, 1),   // Up-Right
		new(1, 0),   // Right
		new(1, -1),  // Down-Right
		new(0, -1),  // Down
		new(-1, -1), // Down-Left
		new(-1, 0),  // Left
		new(-1, 1)   // Up-Left
	};
	
	private void Awake() {
		if (!Instance)
			Instance = this;
		else
			Destroy(gameObject);
	}
    
	public async void Start() {
		await BuildGraph();
		Debug.Log("NavGraph built");
		OnNavGraphBuild?.Invoke();
	}

	public NavGraphNode GetClosestNode(Vector2 position) {
		float minDistance = float.MaxValue;
		NavGraphNode closestNode = null;

		foreach (KeyValuePair<Vector2Int, NavGraphNode> kvp in graphNodes)
		{
			float distance = Vector2.Distance(position * 1000, kvp.Key);
			if (distance < minDistance)
			{
				minDistance = distance;
				closestNode = kvp.Value;
			}
		}
		return closestNode;
	}
	
	public NavGraphNode GetRandomNode() {
		List<Vector2Int> keys = new(graphNodes.Keys);
		Vector2Int randomKey = keys[Random.Range(0, keys.Count)];
		return graphNodes[randomKey];
	}

	private async UniTask BuildGraph() {
		graphNodes.Clear();
		
		pointsToConsider = new HashSet<Vector2Int> { Vector2Int.FloorToInt(startingPoint * 1000) };

		int infiniteLoopBreaker = 0;
		while (pointsToConsider.Count > 0 && infiniteLoopBreaker < 100000) {
			infiniteLoopBreaker++;

			// get an element from hash set
			HashSet<Vector2Int>.Enumerator enumerator = pointsToConsider.GetEnumerator();
			enumerator.MoveNext();
			Vector2Int currentPosition = enumerator.Current;
			
			// check if this point is valid to add to graph
			if (!VerifyPoint((Vector2)currentPosition * graphPrecision)) {
				pointsToConsider.Remove(currentPosition);
				continue;
			}
			
			if (animateGraphBuild)
				await UniTask.WaitForSeconds(animationStepTime);
			
			// if is valid, add this point
			graphNodes.Add(currentPosition, new NavGraphNode(currentPosition));
			pointsToConsider.Remove(currentPosition);

			// get all new positions and add them to set if not already in graph 
			foreach (Vector2Int offset in offsets) {
				Vector2Int newNodePosition = currentPosition + Vector2Int.FloorToInt((Vector2)offset * gridWidth * 1000);
				
				if (graphNodes.ContainsKey(newNodePosition)) {
					// add edges to current point and already added
					if (!graphNodes[currentPosition].Neighbours.Contains(graphNodes[newNodePosition]))
						graphNodes[currentPosition].Neighbours.Add(graphNodes[newNodePosition]);
					if (!graphNodes[newNodePosition].Neighbours.Contains(graphNodes[currentPosition]))
						graphNodes[newNodePosition].Neighbours.Add(graphNodes[currentPosition]);
					continue;
				}

				pointsToConsider.Add(newNodePosition);
			}
		}
	}

	private bool VerifyPoint(Vector2 point) {
		// discard already added point
		if (graphNodes.ContainsKey(Vector2Int.FloorToInt(point * 1000)))
			return false;
		
		// discard point outside level bounds
		if (point.y > topBorder)
			return false;
		if (point.x > rightBorder)
			return false;
		if (point.y < bottomBorder)
			return false;
		if (point.x < leftBorder)
			return false;
		
		// discard point too close to obstacle
		if (Obstacle.IsCircleTouchingAnyObstacle(point, agentRadius))
			return false;
		
		// discard point inside any obstacle
		if (Obstacle.IsPointInsideAnyObstacle(point))
			return false;
		
		return true;
	}

	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(startingPoint, .2f);

		// draw level bounds
		Gizmos.DrawLineList(new[] {
			new Vector3(leftBorder, topBorder),
			new Vector3(rightBorder, topBorder),
			new Vector3(rightBorder, topBorder),
			new Vector3(rightBorder, bottomBorder),
			new Vector3(rightBorder, bottomBorder),
			new Vector3(leftBorder, bottomBorder),
			new Vector3(leftBorder, bottomBorder),
			new Vector3(leftBorder, topBorder),
		});

		Gizmos.color = Color.yellow;
		foreach (Vector2 point in pointsToConsider) {
			Gizmos.DrawSphere(point * graphPrecision, .1f);
		}
		
		Gizmos.color = Color.green;
		foreach (KeyValuePair<Vector2Int,NavGraphNode> navGraphNode in graphNodes) {
			foreach (INode node in navGraphNode.Value.Neighbours)
				Gizmos.DrawLine((Vector2)navGraphNode.Key * graphPrecision, node.Position * graphPrecision);
		}
	}

	public class NavGraphNode : INode {
		
		public Vector2 Position { get; }
		public List<INode> Neighbours { get; } = new();
		
		public NavGraphNode(Vector2 position) => Position = position;
	}
}
