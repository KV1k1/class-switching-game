using UnityEngine;

public class SmokeZone : MonoBehaviour
{
    float radius, damage, tickRate, timeLeft, nextTick;
    ParticleSystem ps;
    GameObject disc;
    bool ending;

    public static void Spawn(Vector3 center, float radius, float damagePerTick, float duration, float tickRate)
    {
        GameObject go = new GameObject("Smoke Zone");
        go.transform.position = new Vector3(center.x, 0f, center.z);

        // Faint ground disc so the damage area is easy to read
        GameObject disc = Fx.MakeQuad(Fx.DiscTex, new Color(0.5f, 0.5f, 0.55f, 0.35f),
            new Vector3(center.x, 0.08f, center.z), Quaternion.Euler(90f, 0f, 0f), radius * 2f);
        disc.transform.SetParent(go.transform, true);

        // Rising smoke puffs, emitted upward from a flat circle
        GameObject smoke = new GameObject("Smoke Particles");
        smoke.transform.SetParent(go.transform, false);
        smoke.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        smoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);   // cone axis points up

        ParticleSystem ps = smoke.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = 1.6f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 2.6f);
        main.startColor = new Color(0.7f, 0.7f, 0.75f, 0.55f);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

        var emission = ps.emission;
        emission.rateOverTime = radius * radius * 3f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 0f;
        shape.radius = radius * 0.9f;
        shape.radiusThickness = 1f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial = Fx.ParticleMaterial;
        ps.Play();

        SmokeZone s = go.AddComponent<SmokeZone>();
        s.radius = radius;
        s.damage = damagePerTick;
        s.tickRate = tickRate;
        s.timeLeft = duration;
        s.ps = ps;
        s.disc = disc;
    }

    void Update()
    {
        if (ending) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            ending = true;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);   // let the last puffs drift away
            Destroy(disc);
            Destroy(gameObject, 1.8f);
            return;
        }

        if (Time.time >= nextTick)
        {
            nextTick = Time.time + tickRate;
            SkillUtil.DamageSphere(transform.position, radius, damage, null);
        }
    }
}