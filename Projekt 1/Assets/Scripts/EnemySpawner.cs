using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : MonoBehaviour {

    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int maxEnemies;
    [SerializeField] private float minSpawnTime = 5f;
    [SerializeField] private float maxSpawnTime = 8f;
    
    private float spawnTimer = 5f;

    private async void Start() {
        await Task.Delay(1000);
        for (int i = 0; i < 5; i++)
            SpawnEnemy();
    }
    
    private void Update() {
        if (!GameManager.Instance.Player)
            return;
        
        spawnTimer -= Time.deltaTime;
        if (spawnTimer >= 0f)
            return;
        
        SpawnEnemy();
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }

    private void SpawnEnemy() {
        if (GameManager.Instance.Objects.Count(entity => entity is Enemy) >= maxEnemies)
            return;
        
        Vector3 spawnPosition;
        do {
            spawnPosition = new Vector3(Random.Range(-15, 15), Random.Range(-15, 15), 0);
        } while (Vector3.Distance(GameManager.Instance.Player.Position, spawnPosition) < 5f);

        Enemy enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.SetParent(transform);
    }
}
