using UnityEngine;

public class ProceduralSlashFx : MonoBehaviour
{
    public float radius = 4f;
    public float duration = 0.3f;
    public Color color = new Color(1f, 0.2f, 0.2f);

    private LineRenderer lr;
    private float timeAlive;

    void Start()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.loop = true;
        lr.positionCount = 36;
        lr.startWidth = 0.5f;
        lr.endWidth = 0.1f;
        // Uses standard sprite shader for a glowing, unlit look
        lr.material = new Material(Shader.Find("Sprites/Default"));

        // Plot a perfect circle
        for (int i = 0; i < 36; i++)
        {
            float angle = i * Mathf.PI * 2 / 36;
            lr.SetPosition(i, new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)));
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;
        float progress = timeAlive / duration;

        if (progress >= 1f)
        {
            Destroy(gameObject);
            return;
        }

        // Rapidly expand outward
        transform.localScale = Vector3.one * Mathf.Lerp(1f, radius, progress);

        // Fade opacity to zero
        Color fadedColor = color;
        fadedColor.a = 1f - progress;
        lr.startColor = fadedColor;
        lr.endColor = fadedColor;
    }
}