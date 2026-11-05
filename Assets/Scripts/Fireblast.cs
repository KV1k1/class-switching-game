using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Mage/Fireblast")]
public class FireblastSkill : Skill
{
    public GameObject fireballPrefab; // Attach a prefab with a Rigidbody and the FireballProjectile script below
    public float projectileSpeed = 15f;
    public float explosionRadius = 4f;
    public float damage = 20f;

    public override IEnumerator Use(PlayerLoadout user)
    {
        Vector3 spawnPos = user.transform.position + Vector3.up * 1f + user.transform.forward * 0.8f;
        GameObject fb = Instantiate(fireballPrefab, spawnPos, user.transform.rotation);

        FireballProjectile script = fb.AddComponent<FireballProjectile>();
        script.Setup(projectileSpeed, explosionRadius, damage, user.gameObject);

        yield break;
    }
}

// Attach this secondary script to your fireball prefab
public class FireballProjectile : MonoBehaviour
{
    float speed, radius, dmg;
    GameObject caster;

    public void Setup(float s, float r, float d, GameObject c)
    {
        speed = s; radius = r; dmg = d; caster = c;
        Destroy(gameObject, 5f); // Cleanup if it misses
    }

    void Update() => transform.position += transform.forward * speed * Time.deltaTime;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == caster || other.isTrigger) return;
        SkillUtil.DamageSphere(transform.position, radius, dmg, caster); // Explode
        Fx.Burst(transform.position, Color.red, 15, radius, 0.5f, 0.5f);
        Destroy(gameObject);
    }
}