using System.Collections;
using UnityEngine;

public class HitFeedback : MonoBehaviour
{
    Health health;
    Renderer rend;
    Vector3 baseScale;
    bool punching;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        health.Damaged += OnDamaged;
        Health.AnyDied += OnAnyDied;
    }

    void OnDisable()
    {
        health.Damaged -= OnDamaged;
        Health.AnyDied -= OnAnyDied;
    }

    Vector3 Center => rend != null ? rend.bounds.center : transform.position;

    void OnDamaged(float amount)
    {
        Fx.Burst(Center, new Color(1f, 0.9f, 0.5f), 6, 4f, 0.15f * baseScale.x, 0.25f);
        if (!punching) StartCoroutine(Punch());
    }

    void OnAnyDied(Health who)
    {
        if (who != health) return;
        Color c = rend != null ? rend.material.color : Color.white;
        Fx.Burst(Center, c, 25, 7f, 0.3f * baseScale.x, 0.6f);
    }

    // Quick squash: the body swells 15% and returns
    IEnumerator Punch()
    {
        punching = true;
        float t = 0f;
        const float len = 0.12f;
        while (t < len)
        {
            if (health.IsDead) yield break;   // don't fight the boss death animation
            t += Time.deltaTime;
            float k = Mathf.Sin(t / len * Mathf.PI);
            transform.localScale = baseScale * (1f + 0.15f * k);
            yield return null;
        }
        transform.localScale = baseScale;
        punching = false;
    }
}