using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))] // REQUIRED: Unity requires a Rigidbody for OnTriggerEnter to detect collisions
public class VfxProjectile : MonoBehaviour
{
    public float speed = 14f;
    public float damage = 12f;
    public float aoeRadius = 1.5f;
    public float lifetime = 3f;
    public GameObject explosionVfxPrefab;

    GameObject caster;
    bool hasExploded;

    public void Init(GameObject owner, float customDamage = -1f, float customSpeed = -1f)
    {
        caster = owner;
        if (customDamage > 0) damage = customDamage;
        if (customSpeed > 0) speed = customSpeed;

        // Configure Rigidbody so physics triggers fire without gravity physics interfering
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Ensure collider is set to trigger
        SphereCollider col = GetComponent<SphereCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        // 1. Ignore Caster & Player
        if (other.gameObject == caster || other.CompareTag("Player")) return;

        // 2. Environment Impact -> Explode on walls/scenery
        if (other.CompareTag("Environment"))
        {
            TriggerExplosion(transform.position, applyDamage: false);
            return;
        }

        // 3. Enemy Impact -> Explode & deal AOE damage
        Health enemyHealth = other.GetComponentInParent<Health>();
        if (enemyHealth != null || other.CompareTag("Enemy"))
        {
            TriggerExplosion(transform.position, applyDamage: true);
        }
    }

    void TriggerExplosion(Vector3 impactPos, bool applyDamage)
    {
        hasExploded = true;

        // Spawn Explosion VFX
        if (explosionVfxPrefab != null)
        {
            GameObject explosion = Instantiate(explosionVfxPrefab, impactPos, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // Apply Damage if hitting an enemy
        if (applyDamage)
        {
            SkillUtil.DamageSphere(impactPos, aoeRadius, damage, caster);

            if (VfxJuice.Instance != null)
            {
                VfxJuice.Instance.ShakeCamera(0.15f, 0.25f);
            }
        }

        Destroy(gameObject);
    }
}