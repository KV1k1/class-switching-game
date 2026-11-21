using UnityEngine;

public class SmokeZone : MonoBehaviour
{
    float radius, damage, tickRate, timeLeft, nextTick;
    ParticleSystem ps;
    GameObject disc;
    GameObject customVfxInstance;
    bool ending;

    public static void Spawn(Vector3 center, float radius, float damagePerTick, float duration, float tickRate, GameObject customVfxPrefab = null)
    {
        GameObject go = new GameObject("Smoke Zone");
        go.transform.position = new Vector3(center.x, 0f, center.z);

        // Ground indicator disc to highlight the damage radius
        GameObject disc = Fx.MakeQuad(Fx.DiscTex, new Color(0.2f, 0.8f, 0.2f, 0.35f), // Green tint for poison
            new Vector3(center.x, 0.08f, center.z), Quaternion.Euler(90f, 0f, 0f), radius * 2f);
        disc.transform.SetParent(go.transform, true);

        ParticleSystem psComponent = null;
        GameObject vfxInstance = null;

        if (customVfxPrefab != null)
        {
            // Instantiate custom poison cloud prefab
            vfxInstance = Instantiate(customVfxPrefab, new Vector3(center.x, 0.1f, center.z), Quaternion.identity, go.transform);
            
            // Adjust scale to match the smokeRadius
            vfxInstance.transform.localScale = Vector3.one * (radius / 2f);
            
            psComponent = vfxInstance.GetComponentInChildren<ParticleSystem>();
        }
        else
        {
            // Fallback: Procedural smoke particles if no prefab is assigned
            GameObject smoke = new GameObject("Smoke Particles");
            smoke.transform.SetParent(go.transform, false);
            smoke.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            smoke.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            psComponent = smoke.AddComponent<ParticleSystem>();
            psComponent.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = psComponent.main;
            main.loop = true;
            main.duration = 1f;
            main.startLifetime = 1.6f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 2.6f);
            main.startColor = new Color(0.2f, 0.8f, 0.3f, 0.55f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);

            var emission = psComponent.emission;
            emission.rateOverTime = radius * radius * 3f;

            var shape = psComponent.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 0f;
            shape.radius = radius * 0.9f;
            shape.radiusThickness = 1f;

            var col = psComponent.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.25f), new GradientAlphaKey(0f, 1f) });
            col.color = g;

            smoke.GetComponent<ParticleSystemRenderer>().sharedMaterial = Fx.ParticleMaterial;
            psComponent.Play();
        }

        SmokeZone s = go.AddComponent<SmokeZone>();
        s.radius = radius;
        s.damage = damagePerTick;
        s.tickRate = tickRate;
        s.timeLeft = duration;
        s.ps = psComponent;
        s.disc = disc;
        s.customVfxInstance = vfxInstance;
    }

    void Update()
    {
        if (ending) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            ending = true;
            if (ps != null) ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            Destroy(disc);
            Destroy(gameObject, 2f);
            return;
        }

        if (Time.time >= nextTick)
        {
            nextTick = Time.time + tickRate;
            SkillUtil.DamageSphere(transform.position, radius, damage, null);
        }
    }
}