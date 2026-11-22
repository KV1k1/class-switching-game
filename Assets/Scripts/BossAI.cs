using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossAI : MonoBehaviour
{
    public string bossName = "THE GUARDIAN";
    public float speed = 2.5f;
    public float wakeRadius = 12f;
    public float stopDistance = 3.5f;
    public float timeBetweenAttacks = 2.5f;

    [Header("Slam (used when the player is close)")]
    public float slamRange = 7f;
    public float slamRadius = 6f;
    public float slamDamage = 25f;
    public float slamWindup = 1f;     // match this to when the Smash clip actually hits
    public float slamRecover = 0.6f;  // time to finish the animation after the hit

    [Header("Projectile ring (used when the player is far)")]
    public int ringBullets = 16;
    public int ringWaves = 2;
    public float ringDamage = 10f;
    public float ringBulletSpeed = 10f;

    [Header("Rewards")]
    public ClassData classToUnlock; // Assign the Mage ClassData in the Inspector
    public int experienceReward = 100;
    public int masteryResourceReward = 25;

    Health health;
    Transform player;
    Renderer rend;
    Animator anim;
    Color originalColor;
    bool awake, attacking;
    float nextAttack;

    void Awake()
    {
        health = GetComponent<Health>();
        health.destroyOnDeath = false; //[cite: 12]
        health.experienceReward = experienceReward;
        health.masteryResourceReward = masteryResourceReward;
    }

    void OnEnable() { Health.AnyDied += OnAnyDied; } //[cite: 12]
    void OnDisable() { Health.AnyDied -= OnAnyDied; } //[cite: 12]

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //[cite: 12]
        rend = GetComponentInChildren<Renderer>(); //[cite: 12]
        originalColor = rend.material.color; //[cite: 12]
        anim = GetComponentInChildren<Animator>(); //[cite: 12]
        if (anim != null) anim.speed = 0f;   // frozen until it wakes up[cite: 12]
        health.Damaged += OnHit; //[cite: 12]
    }

    void OnHit(float amount) { Wake(); } //[cite: 12]

    void Wake()
    {
        if (awake) return; //[cite: 12]
        awake = true; //[cite: 12]
        if (anim != null) anim.speed = 1f; //[cite: 12]
        nextAttack = Time.time + 1.5f; //[cite: 12]
        GameUI.Instance.ShowBossBar(bossName, health); //[cite: 12]
    }

    void Update()
    {
        if (health.IsDead || player == null) return; //[cite: 12]

        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z); //[cite: 12]
        float dist = Vector3.Distance(transform.position, target); //[cite: 12]

        if (!awake)
        {
            if (dist <= wakeRadius) Wake(); //[cite: 12]
            return; //[cite: 12]
        }

        if (attacking) return; //[cite: 12]

        transform.LookAt(target); //[cite: 12]
        if (dist > stopDistance) //[cite: 12]
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime); //[cite: 12]

        if (Time.time >= nextAttack) //[cite: 12]
            StartCoroutine(dist <= slamRange ? Slam() : Ring()); //[cite: 12]
    }

    void Play(string state)
    {
        if (anim != null) anim.CrossFadeInFixedTime(state, 0.15f); //[cite: 12]
    }

    IEnumerator Slam()
    {
        attacking = true; //[cite: 12]
        Vector3 center = new Vector3(transform.position.x, 0f, transform.position.z); //[cite: 12]

        Play("Smash"); //[cite: 12]
        SkillUtil.FlashDisc(center, slamRadius, new Color(1f, 0.3f, 0.1f), slamWindup); //[cite: 12]
        rend.material.color = Color.yellow; //[cite: 12]
        yield return new WaitForSeconds(slamWindup); //[cite: 12]
        rend.material.color = originalColor; //[cite: 12]

        SkillUtil.DamageSphere(center, slamRadius, slamDamage, gameObject, true); //[cite: 12]
        SkillUtil.FlashDisc(center, slamRadius, Color.white, 0.2f); //[cite: 12]

        yield return new WaitForSeconds(slamRecover); //[cite: 12]
        Play("Walk"); //[cite: 12]
        nextAttack = Time.time + timeBetweenAttacks; //[cite: 12]
        attacking = false; //[cite: 12]
    }

    IEnumerator Ring()
    {
        attacking = true; //[cite: 12]

        Play("Fire"); //[cite: 12]
        rend.material.color = Color.yellow; //[cite: 12]
        yield return new WaitForSeconds(0.6f); //[cite: 12]
        rend.material.color = originalColor; //[cite: 12]

        float step = 360f / ringBullets; //[cite: 12]
        float baseAngle = Random.Range(0f, 360f); //[cite: 12]

        for (int w = 0; w < ringWaves; w++) //[cite: 12]
        {
            for (int i = 0; i < ringBullets; i++) //[cite: 12]
            {
                float angle = baseAngle + i * step + (w % 2) * step * 0.5f; //[cite: 12]
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward; //[cite: 12]
                Vector3 pos = new Vector3(transform.position.x, 1f, transform.position.z) + dir * 2.2f; //[cite: 12]
                Projectile.Spawn(pos, dir, ringBulletSpeed, ringDamage, new Color(1f, 0.4f, 0f), true); //[cite: 12]
            }
            yield return new WaitForSeconds(0.6f); //[cite: 12]
        }

        Play("Walk"); //[cite: 12]
        nextAttack = Time.time + timeBetweenAttacks; //[cite: 12]
        attacking = false; //[cite: 12]
    }

    void OnAnyDied(Health who)
    {
        if (who != health) return; //[cite: 12]
        StopAllCoroutines(); //[cite: 12]
        StartCoroutine(Die()); //[cite: 12]
    }

    IEnumerator Die()
    {
        GameProgression.MarkFirstBossDefeated();
        GameUI.Instance.HideBossBar();
        if (anim != null) anim.speed = 0f;

        // 1. Unlock the class and show the unlock notification
        PlayerLoadout currentLoadout = player != null ? player.GetComponent<PlayerLoadout>() : null;
        bool alreadyEquippedReward = currentLoadout != null &&
            System.Array.IndexOf(currentLoadout.classes, classToUnlock) >= 0;
        if (classToUnlock != null && player != null && !alreadyEquippedReward)
        {
            DroppedClassPickup.Spawn(classToUnlock, transform.position);
            GameUI.Instance.ShowNotification($"DROPPED: {classToUnlock.className}!\nMove close and press E to collect");
        }

        // 2. Shrink the boss away in real-time
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 1.5f);
            yield return null;
        }

        // 3. Wait 3.5 seconds so the unlock notification finishes clearing before the wave warning starts
        yield return new WaitForSeconds(3.5f);

        // 4. Trigger the final wave countdown safely
        Transform runPlayer = player;
        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.SpawnFinalWave(20, 1.5f, () => CompleteRun(runPlayer));
        }
        else
        {
            CompleteRun(runPlayer);
        }

        Destroy(gameObject);
    }

    static void CompleteRun(Transform runPlayer)
    {
        PlayerLoadout loadout = runPlayer != null ? runPlayer.GetComponent<PlayerLoadout>() : null;
        PlayerExperience experience = runPlayer != null ? runPlayer.GetComponent<PlayerExperience>() : null;
        int returnedMasteryResource = GameProgression.SaveCompletedRun(
            loadout != null ? loadout.classes : null,
            experience != null ? experience.currentLevel : 1);

        bool archivistRescued = GameProgression.RecruitArchivistAfterCompletedExpedition();
        WorldPortal returnPortal = null;
        if (runPlayer != null)
            returnPortal = WorldPortal.SpawnHubReturn(runPlayer.position + runPlayer.forward * 5f);
        if (archivistRescued && returnPortal != null)
            ArchivistNpcController.SpawnAtReturnPortal(returnPortal.transform.position, runPlayer);

        if (GameUI.Instance != null)
            GameUI.Instance.ShowNotification(archivistRescued
                ? "RUN COMPLETE!\nThe Archivist is waiting at the return portal.\nReturned with " + returnedMasteryResource + " Memory Shards."
                : "RUN COMPLETE!\nReturned with " + returnedMasteryResource + " Memory Shards. A return portal has opened");
    }
}
