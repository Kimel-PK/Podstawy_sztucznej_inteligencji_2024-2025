using UnityEngine;

public class BlockSpawner : MonoBehaviour {
	
	[SerializeField] private Obstacle obstaclePrefab;
	[SerializeField] private float obstacleNumber = 10;
	[SerializeField] private float spawnRange = 10;

	private void Awake() {
		for (int i = 0; i < obstacleNumber; i++) {
			Obstacle obstacle = Instantiate(obstaclePrefab, new Vector3(Random.Range(spawnRange, -spawnRange), Random.Range(spawnRange, -spawnRange), 0), Quaternion.identity);
			obstacle.transform.SetParent(transform);
			obstacle.ColliderRadius = Random.Range(0.5f, 1f);
		}
	}

	private void Start() {
		foreach (Transform child in transform)
			GameManager.Instance.Objects.Add(child);
	}
}