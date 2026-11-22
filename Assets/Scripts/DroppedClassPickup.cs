using UnityEngine;

public class DroppedClassPickup : MonoBehaviour
{
    public float pickupRadius = 2.5f;
    public float bobHeight = 0.18f;
    public float spinSpeed = 50f;

    ClassData classData;
    Transform player;
    PlayerLoadout playerLoadout;
    Vector3 basePosition;
    bool wasInRange;
    float pickupAvailableAt;
    Material ringMaterial;

    public static DroppedClassPickup Spawn(ClassData data, Vector3 position)
    {
        if (data == null) return null;

        GameObject pickupObject = new GameObject(data.className + " Weapon Drop");
        DroppedClassPickup pickup = pickupObject.AddComponent<DroppedClassPickup>();
        pickup.Initialize(data, position);
        return pickup;
    }

    void Initialize(ClassData data, Vector3 position)
    {
        classData = data;
        basePosition = new Vector3(position.x, 0.2f, position.z);
        transform.position = basePosition;
        pickupAvailableAt = Time.time + 0.35f;

        if (classData.weaponPrefab != null)
        {
            GameObject weapon = Instantiate(classData.weaponPrefab, transform);
            weapon.name = classData.className + " Dropped Weapon";
            weapon.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            weapon.transform.localRotation = Quaternion.Euler(classData.weaponRotation);
            weapon.transform.localScale = classData.weaponScale * 1.25f;
            foreach (Collider childCollider in weapon.GetComponentsInChildren<Collider>())
                childCollider.enabled = false;
        }

        Light glow = gameObject.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.25f, 0.8f, 1f);
        glow.intensity = 2f;
        glow.range = 5f;

        CreateGlowRing();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerLoadout = player.GetComponent<PlayerLoadout>();
    }

    void Update()
    {
        transform.position = basePosition + Vector3.up * (bobHeight + Mathf.Sin(Time.time * 2.5f) * bobHeight);
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);

        if (player == null || playerLoadout == null) return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        bool inRange = offset.sqrMagnitude <= pickupRadius * pickupRadius;

        if (inRange)
        {
            wasInRange = true;
            if (GameUI.Instance != null)
                GameUI.Instance.SetPickupPrompt("Press E to collect " + classData.className);

            if (Time.time >= pickupAvailableAt && Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E))
            {
                if (playerLoadout.AcquireClass(classData))
                {
                    if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
                    Destroy(gameObject);
                }
                else if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowNotification(classData.className + " is already equipped");
                }
            }
        }
        else if (wasInRange)
        {
            wasInRange = false;
            if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
        }
    }

    void OnDestroy()
    {
        if (wasInRange && GameUI.Instance != null)
            GameUI.Instance.SetPickupPrompt("");
        if (ringMaterial != null) Destroy(ringMaterial);
    }

    void CreateGlowRing()
    {
        GameObject ringObject = new GameObject("Glow Ring");
        ringObject.transform.SetParent(transform, false);
        ringObject.transform.localPosition = new Vector3(0f, 0.08f, 0f);

        LineRenderer ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 48;
        ring.startWidth = 0.045f;
        ring.endWidth = 0.045f;
        ring.startColor = new Color(0.2f, 0.85f, 1f, 0.9f);
        ring.endColor = new Color(0.55f, 0.95f, 1f, 0.65f);
        ringMaterial = new Material(Shader.Find("Sprites/Default"));
        ring.material = ringMaterial;

        const float radius = 0.9f;
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }
}
