using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public static GameManager Instance { get; set; }

	[SerializeField] private List<Obstacle> obstacles;

	private void Awake() {
		if (!Instance)
			Instance = this;
		else
			Destroy(gameObject);
	}

	// TODO
	public bool IsPointInsideObstacle(Vector2 point) {
		foreach (Obstacle obstacle in obstacles) {
			if (obstacle.IsPointInsideObstacle(point))
				return true;
		}
		return false;
	}
}
