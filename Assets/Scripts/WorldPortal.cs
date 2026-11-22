using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldPortal : MonoBehaviour
{
    public float interactionRange = 3f;
    public string destinationScene;
    public string prompt = "Press E to travel";

    Transform player;
    bool inRange;

    public static WorldPortal SpawnToFirstMap(Vector3 position)
    {
        return Spawn(position, "SampleScene", "Press E to enter the first expedition", "FIRST EXPEDITION");
    }

    public static WorldPortal SpawnHubReturn(Vector3 position)
    {
        return Spawn(position, "HubScene", "Press E to return to the sanctuary", "RETURN TO SANCTUARY");
    }

    static WorldPortal Spawn(Vector3 position, string scene, string interactionPrompt, string portalName)
    {
        GameObject portalObject = new GameObject(portalName + " Portal");
        portalObject.transform.position = new Vector3(position.x, 0f, position.z);
        WorldPortal portal = portalObject.AddComponent<WorldPortal>();
        portal.destinationScene = scene;
        portal.prompt = interactionPrompt;
        portal.CreateVisuals();
        return portal;
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 offset = player.position - transform.position;
        offset.y = 0f;
        bool nowInRange = offset.sqrMagnitude <= interactionRange * interactionRange;
        if (nowInRange)
        {
            inRange = true;
            if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt(prompt);
            if (Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E))
            {
                if (!string.IsNullOrEmpty(destinationScene) && Application.CanStreamedLevelBeLoaded(destinationScene))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(destinationScene);
                }
                else if (GameUI.Instance != null)
                {
                    GameUI.Instance.ShowNotification("The destination scene is not in Build Settings");
                }
            }
        }
        else if (inRange)
        {
            inRange = false;
            if (GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
        }
    }

    void OnDestroy()
    {
        if (inRange && GameUI.Instance != null) GameUI.Instance.SetPickupPrompt("");
    }

    void CreateVisuals()
    {
        Material stone = MakeMaterial(new Color(0.2f, 0.17f, 0.25f));
        for (int side = -1; side <= 1; side += 2)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Portal Pillar";
            pillar.transform.SetParent(transform, false);
            pillar.transform.localPosition = new Vector3(side * 1.7f, 1.45f, 0f);
            pillar.transform.localScale = new Vector3(0.55f, 1.45f, 0.55f);
            Object.Destroy(pillar.GetComponent<Collider>());
            pillar.GetComponent<Renderer>().material = stone;
        }

        GameObject lintel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lintel.name = "Portal Lintel";
        lintel.transform.SetParent(transform, false);
        lintel.transform.localPosition = new Vector3(0f, 3f, 0f);
        lintel.transform.localScale = new Vector3(3.9f, 0.4f, 0.55f);
        Object.Destroy(lintel.GetComponent<Collider>());
        lintel.GetComponent<Renderer>().material = stone;

        GameObject glow = new GameObject("Portal Light");
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = new Vector3(0f, 1.5f, 0f);
        Light portalLight = glow.AddComponent<Light>();
        portalLight.color = new Color(0.15f, 0.75f, 1f);
        portalLight.intensity = 3f;
        portalLight.range = 8f;

        GameObject ringObject = new GameObject("Portal Ring");
        ringObject.transform.SetParent(transform, false);
        LineRenderer ring = ringObject.AddComponent<LineRenderer>();
        ring.useWorldSpace = false;
        ring.loop = true;
        ring.positionCount = 64;
        ring.startWidth = 0.09f;
        ring.endWidth = 0.09f;
        ring.startColor = new Color(0.2f, 0.85f, 1f, 1f);
        ring.endColor = new Color(0.6f, 0.5f, 1f, 1f);
        ring.material = MakeMaterial(Color.white);
        for (int i = 0; i < ring.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / ring.positionCount;
            ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * 1.55f, 1.65f + Mathf.Sin(angle) * 1.65f, 0f));
        }
    }

    static Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 1.5f);
        }
        return material;
    }
}
