using UnityEngine;

public static class SkillUtil
{
    public static void DamageSphere(Vector3 center, float radius, float damage, GameObject ignore, bool targetPlayer = false)
    {
        foreach (Collider hit in Physics.OverlapSphere(center, radius))
        {
            if (hit.gameObject == ignore) continue;
            if (hit.CompareTag("Player") != targetPlayer) continue;

            Health h = hit.GetComponent<Health>();
            if (h != null) h.TakeDamage(damage);
        }
    }

    public static void FlashDisc(Vector3 center, float radius, Color color, float duration)
    {
        bool warning = duration >= 0.5f;   // long = telegraph (stays full size), short = impact pop
        color.a = warning ? 0.6f : 0.85f;

        GameObject fx = Fx.MakeQuad(Fx.DiscTex, color,
            new Vector3(center.x, 0.12f, center.z),
            Quaternion.Euler(90f, 0f, 0f),
            radius * 2f);

        FxFade.Attach(fx, duration, warning ? 1f : 0.6f, 1f, warning ? 0.85f : 0f);
    }
}