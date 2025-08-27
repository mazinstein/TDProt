using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public Transform targetPoint; // The place all enemies go to

    public float spawnInterval = 3f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        int idx = Random.Range(0, spawnPoints.Length);
        var enemyObj = Instantiate(enemyPrefab, spawnPoints[idx].position, Quaternion.identity);
        var enemyAI = enemyObj.GetComponent<EnemyAI>();
        if (enemyAI != null)
            enemyAI.SetTarget(targetPoint);
    }
}