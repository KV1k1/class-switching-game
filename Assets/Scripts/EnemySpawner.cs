using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 1.7f;
    [Min(0.2f)] public float minimumSpawnInterval = 0.75f;
    [Min(1f)] public float spawnRateGrowthPerMinute = 1.22f;
    [Min(1)] public int maxAliveZombies = 12;
    public float spawnRadius = 15f;
    [Header("Timed boss encounter")]
    [Min(1f)] public float bossSpawnDelaySeconds = 60f;
    Transform player;
    BossAI timedBoss;
    Coroutine zombieSpawnRoutine;
    readonly List<GameObject> regularZombies = new List<GameObject>();
    float expeditionStartTime;

    bool finalWaveActive = false;
    List<GameObject> finalWaveEnemies = new List<GameObject>();
    System.Action onFinalWaveDefeated;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene") return;

        timedBoss = FindFirstObjectByType<BossAI>();
        if (timedBoss != null) timedBoss.gameObject.SetActive(false);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "SampleScene") return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null) return;
        player = playerObject.transform;
        expeditionStartTime = Time.time;
        zombieSpawnRoutine = StartCoroutine(SpawnZombies());
        if (timedBoss != null) StartCoroutine(SpawnTimedBoss());
    }

    IEnumerator SpawnZombies()
    {
        yield return new WaitForSeconds(1f);
        while (!finalWaveActive && player != null)
        {
            Spawn();
            yield return new WaitForSeconds(CalculateSpawnInterval());
        }
        zombieSpawnRoutine = null;
    }

    float CalculateSpawnInterval()
    {
        float elapsedMinutes = Mathf.Max(0f, Time.time - expeditionStartTime) / 60f;
        float rateMultiplier = Mathf.Pow(Mathf.Max(1f, spawnRateGrowthPerMinute), elapsedMinutes);
        float baseInterval = Mathf.Max(0.2f, spawnInterval);
        return Mathf.Max(Mathf.Max(0.2f, minimumSpawnInterval), baseInterval / rateMultiplier);
    }

    IEnumerator SpawnTimedBoss()
    {
        yield return new WaitForSeconds(Mathf.Max(1f, bossSpawnDelaySeconds));
        if (timedBoss == null || player == null) yield break;

        float spawnDistance = Mathf.Clamp(timedBoss.wakeRadius * 0.65f, 5f, 9f);
        Vector3 spawnPosition = player.position + player.forward * spawnDistance;
        spawnPosition.y = timedBoss.transform.position.y;
        timedBoss.transform.position = spawnPosition;
        Vector3 towardPlayer = player.position - spawnPosition;
        towardPlayer.y = 0f;
        if (towardPlayer.sqrMagnitude > 0.001f)
            timedBoss.transform.rotation = Quaternion.LookRotation(towardPlayer.normalized);

        timedBoss.gameObject.SetActive(true);
        timedBoss.BeginEncounter(player);
        if (GameUI.Instance != null)
            GameUI.Instance.ShowExpeditionStatus("BOSS ENCOUNTER: " + timedBoss.bossName + " has arrived", 4f);
    }

    void Spawn()
    {
        if (finalWaveActive || player == null) return;
        if (enemyPrefab == null) return;

        regularZombies.RemoveAll(zombie => zombie == null);
        if (regularZombies.Count >= Mathf.Max(1, maxAliveZombies)) return;

        Vector2 circle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = player.position + new Vector3(circle.x, 0, circle.y);
        pos.y = 0.1f;
        regularZombies.Add(Instantiate(enemyPrefab, pos, Quaternion.identity));
    }

    // Triggered when the boss dies with a preparation warning
    public void SpawnFinalWave(int count, float speedMultiplier, System.Action onComplete)
    {
        // Stop normal continuous spawning
        if (zombieSpawnRoutine != null)
        {
            StopCoroutine(zombieSpawnRoutine);
            zombieSpawnRoutine = null;
        }
        onFinalWaveDefeated = onComplete;

        // Start preparation warning sequence
        StartCoroutine(FinalWaveCountdownRoutine(count, speedMultiplier));
    }

    IEnumerator FinalWaveCountdownRoutine(int count, float speedMultiplier)
    {
        // Notification phases for the player to prepare
        GameUI.Instance?.ShowExpeditionStatus("FINAL WAVE INCOMING — PREPARE", 1.5f);
        yield return new WaitForSeconds(1.5f);

        GameUI.Instance?.ShowExpeditionStatus("FINAL WAVE STARTS IN 3", 1.2f);
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance?.ShowExpeditionStatus("FINAL WAVE STARTS IN 2", 1.2f);
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance?.ShowExpeditionStatus("FINAL WAVE STARTS IN 1", 1.2f);
        yield return new WaitForSeconds(1.0f);

        GameUI.Instance?.ShowExpeditionStatus("FINAL WAVE STARTED — DEFEAT ALL ENEMIES", 4f);

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
