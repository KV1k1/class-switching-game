using UnityEngine;

public static class Fx
{
    static Texture2D soft, disc, arc;
    static Material particleMat, plainMat;

    // ---------- textures (drawn in code) ----------
    public static Texture2D SoftTex => soft != null ? soft : soft = MakeTexture(64, (u, v) =>
    {
        float d = Mathf.Sqrt(u * u + v * v);
        return Mathf.Clamp01(1f - d);                         // soft round blob
    });

    public static Texture2D DiscTex => disc != null ? disc : disc = MakeTexture(128, (u, v) =>
    {
        float d = Mathf.Sqrt(u * u + v * v);
        if (d >= 1f) return 0f;
        return Mathf.Lerp(0.3f, 1f, Mathf.SmoothStep(0.75f, 1f, d)) * Mathf.Clamp01((1f - d) * 20f);  // faint fill, bright rim
    });

    // A crescent pointing "up" in the texture (we rotate it to face forward)
    public static Texture2D ArcTex => arc != null ? arc : arc = MakeTexture(128, (u, v) =>
    {
        float d = Mathf.Sqrt(u * u + v * v);
        float angle = Mathf.Abs(Mathf.Atan2(u, v) * Mathf.Rad2Deg);   // 0 = straight ahead
        float spread = 75f;
        if (d > 1f || angle > spread) return 0f;
        float a1 = 1f - angle / spread;                               // 1 in the middle, 0 at the tips
        float inner = Mathf.Lerp(0.95f, 0.5f, a1);                    // thick in the middle, thin at the tips
        if (d < inner) return 0f;
        float a = Mathf.Clamp01((d - inner) / (1f - inner));          // brightest on the outer edge
        a *= Mathf.Clamp01((1f - d) * 25f);                           // soften the outer edge
        return a;
    });

    static Texture2D MakeTexture(int size, System.Func<float, float, float> alphaAt)
    {
        Texture2D t = new Texture2D(size, size, TextureFormat.RGBA32, false);
        t.wrapMode = TextureWrapMode.Clamp;
        t.hideFlags = HideFlags.HideAndDontSave;
        Color[] px = new Color[size * size];
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1) * 2f - 1f;
                float v = y / (float)(size - 1) * 2f - 1f;
                px[y * size + x] = new Color(1f, 1f, 1f, alphaAt(u, v));
            }
        t.SetPixels(px);
        t.Apply();
        return t;
    }

    // ---------- shared materials ----------
    public static Material ParticleMaterial
    {
        get
        {
            if (particleMat == null)
            {
                particleMat = new Material(Shader.Find("Sprites/Default"));
                particleMat.mainTexture = SoftTex;
                particleMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return particleMat;
        }
    }

    public static Material PlainMaterial
    {
        get
        {
            if (plainMat == null)
            {
                plainMat = new Material(Shader.Find("Sprites/Default"));
                plainMat.hideFlags = HideFlags.HideAndDontSave;
            }
            return plainMat;
        }
    }

    // ---------- building blocks ----------
    // A textured quad with its own material (so it can be tinted and faded on its own)
    public static GameObject MakeQuad(Texture2D tex, Color color, Vector3 pos, Quaternion rot, float size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Object.Destroy(go.GetComponent<Collider>());
        go.transform.SetPositionAndRotation(pos, rot);
        go.transform.localScale = new Vector3(size, size, 1f);

        Material m = new Material(Shader.Find("Sprites/Default"));
        m.mainTexture = tex;
        m.color = color;
        Renderer r = go.GetComponent<Renderer>();
        r.sharedMaterial = m;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        r.receiveShadows = false;
        return go;
    }

    // A crescent that sweeps out in front of a character
    public static void SlashArc(Transform who, float range, Color color, bool mirror = false)
    {
        Quaternion rot = Quaternion.LookRotation(who.forward, Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        GameObject go = MakeQuad(ArcTex, color, who.position, rot, range * 2f);
        if (mirror)
        {
            Vector3 s = go.transform.localScale;
            s.x = -s.x;
            go.transform.localScale = s;
        }
        FxFade.Attach(go, 0.2f, 0.7f, 1.1f, 0.3f);
    }

    // A one-shot puff of sparks
    public static void Burst(Vector3 pos, Color color, int count = 12, float speed = 5f, float size = 0.25f, float life = 0.4f)
    {
        GameObject go = new GameObject("Burst");
        go.transform.position = pos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // must be stopped before editing settings

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = life;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.5f, size);
        main.startColor = color;
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.enabled = false;                 // we emit by hand below

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                  new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = g;

        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        go.GetComponent<ParticleSystemRenderer>().sharedMaterial = ParticleMaterial;

        ps.Play();
        ps.Emit(count);
        Object.Destroy(go, life + 0.5f);
    }
}