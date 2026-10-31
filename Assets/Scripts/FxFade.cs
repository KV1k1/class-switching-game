using UnityEngine;

public class FxFade : MonoBehaviour
{
    Material mat;
    Color color;
    float duration, hold, t;
    Vector3 startScale, endScale;

    // startMul/endMul: size multipliers over the lifetime.
    // hold: fraction of the lifetime to stay fully visible before fading.
    public static void Attach(GameObject go, float duration, float startMul, float endMul, float hold = 0f)
    {
        FxFade f = go.AddComponent<FxFade>();
        f.mat = go.GetComponent<Renderer>().sharedMaterial;
        f.color = f.mat.color;
        f.duration = duration;
        f.hold = hold;
        f.startScale = go.transform.localScale * startMul;
        f.endScale = go.transform.localScale * endMul;
        go.transform.localScale = f.startScale;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);

        float ease = 1f - (1f - k) * (1f - k);   // fast start, slow finish
        transform.localScale = Vector3.Lerp(startScale, endScale, ease);

        float fade = k <= hold ? 1f : 1f - (k - hold) / (1f - hold);
        Color c = color;
        c.a = color.a * fade;
        mat.color = c;

        if (k >= 1f) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (mat != null) Destroy(mat);
    }
}