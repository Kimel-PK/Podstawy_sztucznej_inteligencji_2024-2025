using UnityEngine;
using Random = UnityEngine.Random;

public class CollectiblesSpawner : MonoBehaviour {
    
    [SerializeField] private NavGraph navGraph;

    [SerializeField] private MedKit medKitPrefab;
    [SerializeField] private ArmorKit armorKitPrefab;
    [SerializeField] private AmmoKit ammoKitPrefab;

    [SerializeField] private int maxItemsOnMap = 5;
    [SerializeField] private float spawnDelay = 10f;

    private bool isActive;
    private float timer;
    
    private void OnEnable() {
        NavGraph.OnNavGraphBuild += NavGraph_OnNavGraphBuild;
    }

    private void OnDisable() {
        NavGraph.OnNavGraphBuild -= NavGraph_OnNavGraphBuild;
    }

    private void Update() {
        if (!isActive)
            return;

        if (timer >= 0f) {
            timer -= Time.deltaTime;
            return;
        }

        timer = spawnDelay;
        SpawnRandomCollectible();
    }

    private void SpawnRandomCollectible() {
        if (MedKit.All.Count + ArmorKit.All.Count + AmmoKit.All.Count >= maxItemsOnMap)
            return;
        
        NavGraph.NavGraphNode randomNode = navGraph.GetRandomNode();

        if (AmmoKit.All.Count == 0) {
            Instantiate(ammoKitPrefab, randomNode.Position / 1000f, Quaternion.identity, transform);
            return;
        }

        if (MedKit.All.Count == 0) {
            Instantiate(medKitPrefab, randomNode.Position / 1000f, Quaternion.identity, transform);
            return;
        }

        switch (Random.Range(0, 3)) {
            case 0:
                Instantiate(medKitPrefab, randomNode.Position / 1000f, Quaternion.identity, transform);
                break;
            case 1:
                Instantiate(armorKitPrefab, randomNode.Position / 1000f, Quaternion.identity, transform);
                break;
            case 2:
                Instantiate(ammoKitPrefab, randomNode.Position / 1000f, Quaternion.identity, transform);
                break;
        }
    }

    private void NavGraph_OnNavGraphBuild() {
        isActive = true;
    }
}
