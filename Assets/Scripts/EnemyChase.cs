using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyChase : MonoBehaviour
{
    public float speed = 3f;
    public float attackRange = 1.8f;
    public float attackDamage = 8f;
    public float windup = 0.4f;          // warning time before the hit lands
    public float recover = 0.5f;         // pause after the hit so the animation can finish
    public float attackCooldown = 1.2f;
    public float deathLinger = 2f;       // how long the body stays after dying

    Transform player;
    Health playerHealth;
    Health health;
    Renderer[] rends;
    Color[] originals;
    Animator anim;
    float nextAttack;
    bool attacking;

    void Awake()
    {
        health = GetComponent<Health>();
        health.destroyOnDeath = false;   // we play the death animation first
    }

    void OnEnable()  { Health.AnyDied += OnAnyDied; }
    void OnDisable() { Health.AnyDied -= OnAnyDied; }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        player = p.transform;
        playerHealth = p.GetComponent<Health>();

        anim = GetComponentInChildren<Animator>();
        rends = GetComponentsInChildren<Renderer>();
        originals = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++) originals[i] = rends[i].material.color;

        Play("Run");
    }

    void Update()
    {
        if (health.IsDead || player == null || attacking) return;

        Vector3 target = new Vector3(player.position.x, transform.position.y, player.position.z);
        float dist = Vector3.Distance(transform.position, target);
        transform.LookAt(target);

        if (dist > attackRange * 0.8f)
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (dist <= attackRange && Time.time >= nextAttack)
            StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        attacking = true;
        Play("Attack");

        Tint(Color.yellow);                       // warning so the player can dodge
        yield return new WaitForSeconds(windup);
        Restore();

        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange + 0.5f)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(recover);
        Play("Run");
        nextAttack = Time.time + attackCooldown;
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
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;   // can't be hit or bumped any more
        Restore();
        Play("Death");
        yield return new WaitForSeconds(deathLinger);
        Destroy(gameObject);
    }

    void Play(string state)
    {
        if (anim != null) anim.CrossFadeInFixedTime(state, 0.1f);
    }

    void Tint(Color c)
    {
        foreach (Renderer r in rends) r.material.color = c;
    }

    void Restore()
    {
        for (int i = 0; i < rends.Length; i++) rends[i].material.color = originals[i];
    }
}