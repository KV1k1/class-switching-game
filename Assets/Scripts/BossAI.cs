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
        health.destroyOnDeath = false;
    }

    void OnEnable()  { Health.AnyDied += OnAnyDied; }
    void OnDisable() { Health.AnyDied -= OnAnyDied; }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;
        anim = GetComponentInChildren<Animator>();
        if (anim != null) anim.speed = 0f;   // frozen until it wakes up
        health.Damaged += OnHit;
    }

    void OnHit(float amount) { Wake(); }

    void Wake()
    {
        if (awake) return;
        awake = true;
        if (anim != null) anim.speed = 1f;
        nextAttack = Time.time + 1.5f;
        GameUI.Instance.ShowBossBar(bossName, health);
    }

    void Update()
    {
        if (health.IsDead || player == null) return;

        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        float dist = Vector3.Distance(transform.position, target);

        if (!awake)
        {
            if (dist <= wakeRadius) Wake();
            return;
        }

        if (attacking) return;

        transform.LookAt(target);
        if (dist > stopDistance)
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Time.time >= nextAttack)
            StartCoroutine(dist <= slamRange ? Slam() : Ring());
    }

    void Play(string state)
    {
        if (anim != null) anim.CrossFadeInFixedTime(state, 0.15f);
    }

    IEnumerator Slam()
    {
        attacking = true;
        Vector3 center = new Vector3(transform.position.x, 0f, transform.position.z);

        Play("Smash");
        SkillUtil.FlashDisc(center, slamRadius, new Color(1f, 0.3f, 0.1f), slamWindup);
        rend.material.color = Color.yellow;
        yield return new WaitForSeconds(slamWindup);
        rend.material.color = originalColor;

        SkillUtil.DamageSphere(center, slamRadius, slamDamage, gameObject, true);
        SkillUtil.FlashDisc(center, slamRadius, Color.white, 0.2f);

        yield return new WaitForSeconds(slamRecover);
        Play("Walk");
        nextAttack = Time.time + timeBetweenAttacks;
        attacking = false;
    }

    IEnumerator Ring()
    {
        attacking = true;

        Play("Fire");
        rend.material.color = Color.yellow;
        yield return new WaitForSeconds(0.6f);
        rend.material.color = originalColor;

        float step = 360f / ringBullets;
        float baseAngle = Random.Range(0f, 360f);

        for (int w = 0; w < ringWaves; w++)
        {
            for (int i = 0; i < ringBullets; i++)
            {
                float angle = baseAngle + i * step + (w % 2) * step * 0.5f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                Vector3 pos = new Vector3(transform.position.x, 1f, transform.position.z) + dir * 2.2f;
                Projectile.Spawn(pos, dir, ringBulletSpeed, ringDamage, new Color(1f, 0.4f, 0f), true);
            }
            yield return new WaitForSeconds(0.6f);
        }

        Play("Walk");
        nextAttack = Time.time + timeBetweenAttacks;
        attacking = false;
    }

    void OnAnyDied(Health who)
    {
        if (who != health) return;
        StopAllCoroutines();
        StartCoroutine(Die());
    }

    IEnumerator Die()
    {
        GameUI.Instance.HideBossBar();
        if (anim != null) anim.speed = 0f;   // freeze mid-pose while it shrinks away

        EnemySpawner spawner = FindFirstObjectByType<EnemySpawner>();
        if (spawner != null) spawner.CancelInvoke();
        foreach (EnemyChase e in FindObjectsByType<EnemyChase>(FindObjectsSortMode.None))
            Destroy(e.gameObject);

        Time.timeScale = 0.3f;
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < 1.5f)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / 1.5f);
            yield return null;
        }

        GameUI.Instance.ShowEnd("GG WP", "Press R to play again", new Color(1f, 0.85f, 0.2f));
        Time.timeScale = 0f;
    }
}