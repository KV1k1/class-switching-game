using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 15f;
    Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(Spawn), 1f, spawnInterval);
    }

    void Spawn()
    {
        if (player == null) return;

        Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);
        pos.y = 1f;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }
}