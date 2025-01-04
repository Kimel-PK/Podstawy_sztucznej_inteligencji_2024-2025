using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { get; set; }

	[SerializeField] private Transform obstaclesParent;
	[SerializeField] private List<Obstacle> obstacles;

	private void Awake() {
		if (!Instance)
			Instance = this;
		else
			Destroy(gameObject);
	}

	private void Start() {
		if (obstaclesParent)
			obstacles.AddRange(obstaclesParent.GetComponentsInChildren<Obstacle>());
	}

	public bool IsCircleTouchingObstacle(Vector2 point, float agentRadius) {
		foreach (Obstacle obstacle in obstacles) {
			if (obstacle.IsCircleTouchingObstacle(point, agentRadius))
				return true;
		}
		return false;
	}

	public bool IsPointInsideObstacle(Vector2 point) {
		foreach (Obstacle obstacle in obstacles) {
			if (obstacle.IsPointInsideObstacle(point))
				return true;
		}
		return false;
	}
}
