using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 direction;
    float speed;
    float damage;
    bool targetPlayer;
    Color color;

    public static void Spawn(Vector3 position, Vector3 direction, float speed, float damage, Color color, bool targetPlayer = false)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.3f;
        go.GetComponent<Renderer>().material.color = color;
        go.GetComponent<SphereCollider>().isTrigger = true;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // Glowing trail behind the bullet
        TrailRenderer tr = go.AddComponent<TrailRenderer>();
        tr.sharedMaterial = Fx.PlainMaterial;
        tr.time = 0.18f;
        tr.startWidth = 0.25f;
        tr.endWidth = 0f;
        tr.minVertexDistance = 0.05f;
        tr.startColor = color;
        tr.endColor = new Color(color.r, color.g, color.b, 0f);
        tr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        Projectile p = go.AddComponent<Projectile>();
        p.direction = direction.normalized;
        p.speed = speed;
        p.damage = damage;
        p.targetPlayer = targetPlayer;
        p.color = color;

        Destroy(go, 2f);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") != targetPlayer) return;

        Health h = other.GetComponent<Health>();
        if (h == null) return;

        h.TakeDamage(damage);
        Fx.Burst(transform.position, color, 8, 4f, 0.15f, 0.25f);   // impact sparks
        Destroy(gameObject);
    }
}