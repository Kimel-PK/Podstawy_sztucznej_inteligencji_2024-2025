using System.Collections.Generic;
using UnityEngine;

public class BlockSpawner : MonoBehaviour {
	
	[SerializeField] private Obstacle obstaclePrefab;
	[SerializeField] private int obstacleNumber = 10;
	
	[SerializeField] private Vector2 spawnRange = new(10, 10);
	[SerializeField] private float minRadius = 0.5f;
	[SerializeField] private float maxRadius = 1f;
	[SerializeField] private float agentMaxRadius = 0.5f;
	
	private readonly List<Obstacle> spawnedObstacles = new();

	private void Awake()
	{
		SpawnObstacles();
	}

	private void SpawnObstacles()
	{
		int attempts = 0;
		int maxAttempts = obstacleNumber * 10; // to avoid an infinite loop if there is no space left

		while (spawnedObstacles.Count < obstacleNumber && attempts < maxAttempts)
		{
			attempts++;

			// Generate random position within the specified range
			float x = Random.Range(-spawnRange.x, spawnRange.x);
			float y = Random.Range(-spawnRange.y, spawnRange.y);
			Vector3 spawnPosition = new(x, y, 0);

			// Generate a random diameter and calculate radius
			float radius = Random.Range(minRadius, maxRadius);

			// Check for overlap with existing spheres
			if (IsOverlapping(spawnPosition, radius))
				continue;
			
			Obstacle obstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
			
			obstacle.transform.SetParent(transform);
			spawnedObstacles.Add(obstacle);
				
			obstacle.SetRadius(radius);
		}
	}

	private bool IsOverlapping(Vector3 newPosition, float newRadius)
	{
		for (int i = 0; i < spawnedObstacles.Count; i++)
		{
			float distance = Vector3.Distance(newPosition, spawnedObstacles[i].Position);
			float minDistance = newRadius + spawnedObstacles[i].ColliderRadius + agentMaxRadius * 2;

			if (distance < minDistance)
				return true;
		}
		return false;
	}
}