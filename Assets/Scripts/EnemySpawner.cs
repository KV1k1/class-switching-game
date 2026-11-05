using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 15f;
    Transform player;

    bool finalWaveActive = false;
    List<GameObject> finalWaveEnemies = new List<GameObject>();
    System.Action onFinalWaveDefeated;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(Spawn), 1f, spawnInterval);
    }

    void Spawn()
    {
        if (finalWaveActive || player == null) return;

        Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);
        pos.y = 0.1f;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    // Triggered when the boss dies with a preparation warning
    public void SpawnFinalWave(int count, float speedMultiplier, System.Action onComplete)
    {
        // Stop normal continuous spawning
        CancelInvoke(nameof(Spawn));
        onFinalWaveDefeated = onComplete;

        // Start preparation warning sequence
        StartCoroutine(FinalWaveCountdownRoutine(count, speedMultiplier));
    }

    IEnumerator FinalWaveCountdownRoutine(int count, float speedMultiplier)
    {
        // Notification phases for the player to prepare
        GameUI.Instance.ShowNotification("PREPARE FOR FINAL WAVE...\nGet Ready!");
        yield return new WaitForSeconds(1.5f);

        GameUI.Instance.ShowNotification("3...");
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance.ShowNotification("2...");
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance.ShowNotification("1...");
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance.ShowNotification("FINAL WAVE SPAWNED!");

        finalWaveActive = true;
        finalWaveEnemies.Clear();

        if (player == null) yield break;

        // Spawn all zombies further away (increased radius multiplier: e.g., 2.5x to 3x further)
        float extendedRadius = spawnRadius * 2.5f;

        for (int i = 0; i < count; i++)
        {
            // Spawn further out in a wider ring
            Vector2 circle = Random.insideUnitCircle.normalized * extendedRadius;
            Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);
            pos.y = 0.1f;

            GameObject zombie = Instantiate(enemyPrefab, pos, Quaternion.identity);

            // Boost the movement speed of the final wave zombies
            EnemyChase chaseScript = zombie.GetComponent<EnemyChase>();
            if (chaseScript != null)
            {
                chaseScript.speed *= speedMultiplier;
            }

            finalWaveEnemies.Add(zombie);
        }
    }

    void Update()
    {
        if (!finalWaveActive) return;

        // Clean up destroyed/null enemies from our tracking list
        finalWaveEnemies.RemoveAll(e => e == null);

        // Once all 20 zombies are defeated, trigger victory
        if (finalWaveEnemies.Count == 0)
        {
            finalWaveActive = false;
            onFinalWaveDefeated?.Invoke();
        }
    }
}