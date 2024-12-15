using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class NavGraph : MonoBehaviour {

	public static Action OnNavGraphBuild = delegate { };

	[SerializeField] private Vector2 startingPoint;
	[SerializeField] private float gridWidth;
	
	[SerializeField] private float topBorder;
	[SerializeField] private float rightBorder;
	[SerializeField] private float bottomBorder;
	[SerializeField] private float leftBorder;

	[SerializeField] private bool animateGraphBuild;
	[SerializeField] private float animationStepTime;
	
	private readonly Dictionary<Vector2, NavGraphNode> graphNodes = new();
	
	// exposed because of gizmo animation
	private HashSet<Vector2> pointsToConsider = new();
	
	private readonly Vector2[] offsets = {
		new(0, 1),   // Up
		new(1, 1),   // Up-Right
		new(1, 0),   // Right
		new(1, -1),  // Down-Right
		new(0, -1),  // Down
		new(-1, -1), // Down-Left
		new(-1, 0),  // Left
		new(-1, 1)   // Up-Left
	};
	
	public async void Start() {
		await BuildGraph();
		Debug.Log("NavGraph built");
		OnNavGraphBuild?.Invoke();
	}

	private async UniTask BuildGraph() {
		graphNodes.Clear();
		
		pointsToConsider = new HashSet<Vector2> { startingPoint };

		int infiniteLoopBreaker = 0;
		while (pointsToConsider.Count > 0 && infiniteLoopBreaker < 100000) {
			infiniteLoopBreaker++;

			// get an element from hash set
			HashSet<Vector2>.Enumerator enumerator = pointsToConsider.GetEnumerator();
			enumerator.MoveNext();
			Vector2 currentPosition = enumerator.Current;
			
			// check if this point is valid to add to graph
			if (!VerifyPoint(currentPosition)) {
				pointsToConsider.Remove(currentPosition);
				continue;
			}
			
			if (animateGraphBuild)
				await UniTask.WaitForSeconds(animationStepTime);
			
			// if is valid, add this point
			graphNodes.Add(currentPosition, new NavGraphNode(currentPosition));
			pointsToConsider.Remove(currentPosition);

			// get all new positions and add them to set if not already in graph 
			foreach (Vector2 offset in offsets) {
				Vector2 newNodePosition = currentPosition + offset * gridWidth;
				
				if (graphNodes.ContainsKey(newNodePosition)) {
					// add edges to current point and already added
					graphNodes[currentPosition].Neighbours.Add(graphNodes[newNodePosition]);
					graphNodes[newNodePosition].Neighbours.Add(graphNodes[currentPosition]);
					continue;
				}

				pointsToConsider.Add(newNodePosition);
			}
		}
	}

	private bool VerifyPoint(Vector2 point) {
		// discard already added point
		if (graphNodes.ContainsKey(point))
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
		
		// discard point inside any obstacle
		// TODO return false if point is in obstacle
		if (GameManager.Instance.IsPointInsideObstacle(point))
			return false;
		
		return true;
	}

	private void OnDrawGizmos() {
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
			Gizmos.DrawSphere(point, .1f);
		}
		
		Gizmos.color = Color.green;
		foreach (KeyValuePair<Vector2,NavGraphNode> navGraphNode in graphNodes) {
			Gizmos.DrawSphere(navGraphNode.Key, .1f);
			foreach (INode node in navGraphNode.Value.Neighbours)
				Gizmos.DrawLine(navGraphNode.Key, node.Position);
		}
	}

	public class NavGraphNode : INode {
		
		public Vector2 Position { get; }
		public List<INode> Neighbours { get; } = new();
		
		public NavGraphNode(Vector2 position) => Position = position;
	}
}
