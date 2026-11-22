using UnityEngine;

public static class HubWorldController
{
    const string HubRootName = "Sanctuary Woodland";

    public static void Initialize(Health playerHealth)
    {
        foreach (EnemySpawner spawner in Object.FindObjectsOfType<EnemySpawner>())
        {
            spawner.CancelInvoke();
            spawner.enabled = false;
        }

        foreach (BossAI boss in Object.FindObjectsOfType<BossAI>())
            boss.gameObject.SetActive(false);

        foreach (EnemyChase enemy in Object.FindObjectsOfType<EnemyChase>())
            enemy.gameObject.SetActive(false);

        if (playerHealth != null)
        {
            CharacterController controller = playerHealth.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            playerHealth.transform.position = new Vector3(0f, 1f, -8f);
            playerHealth.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            if (controller != null) controller.enabled = true;
        }

        BuildWoodlandClearing();
        CreateSanctuaryChronicle();
        WorldPortal.SpawnToFirstMap(new Vector3(0f, 0f, 20f));
        if (GameProgression.ArchivistRecruited)
            ArchivistNpcController.SpawnForHub(new Vector3(-5f, 0f, 0f));
        CreateResident("The Blacksmith", "Bring back strange weapons. I can help you make sense of them.",
            new Vector3(5f, 0f, 0f), new Color(0.8f, 0.34f, 0.2f));
    }

    static void BuildWoodlandClearing()
    {
        GameObject oldRoot = GameObject.Find(HubRootName);
        if (oldRoot != null) Object.Destroy(oldRoot);

        GameObject root = new GameObject(HubRootName);
        ArenaBuilder assetCatalog = Object.FindFirstObjectByType<ArenaBuilder>();
        GameObject[] trees = assetCatalog != null && assetCatalog.sanctuaryTreePrefabs != null &&
            assetCatalog.sanctuaryTreePrefabs.Length > 0
            ? assetCatalog.sanctuaryTreePrefabs
            : assetCatalog != null ? assetCatalog.treePrefabs : null;

        // Use the CommonTree prefab already wired into the project scene. A gap
        // to the north keeps the expedition portal visible from the spawn.
        if (trees != null && trees.Length > 0)
        {
            for (int i = 0; i < 36; i++)
            {
                float angle = i * Mathf.PI * 2f / 36f;
                float radius = Random.Range(22f, 29f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (position.z > 15f && Mathf.Abs(position.x) < 8f) continue;

                GameObject prefab = trees[Random.Range(0, trees.Length)];
                if (prefab == null) continue;
                GameObject tree = Object.Instantiate(prefab, position,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), root.transform);
                tree.name = "Sanctuary Grove Tree";
                float scale = Random.Range(0.85f, 1.15f);
                tree.transform.localScale *= scale;
            }
        }

        GameObject[] details = assetCatalog != null ? assetCatalog.sanctuaryDetailPrefabs : null;
        if (details != null && details.Length > 0)
        {
            for (int i = 0; i < 24; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float radius = Random.Range(10f, 17f);
                Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                if (Mathf.Abs(position.x) < 4.5f && position.z > -9f && position.z < 21f) continue;

                GameObject prefab = details[Random.Range(0, details.Length)];
                if (prefab == null) continue;
                GameObject decoration = Object.Instantiate(prefab, position,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), root.transform);
                decoration.name = "Sanctuary Woodland Detail";
                decoration.transform.localScale *= Random.Range(0.75f, 1.25f);
            }
        }

        // A simple stone path connects the entrance, residents, and portal.
        Material pathMaterial = CreateMaterial(new Color(0.21f, 0.19f, 0.2f));
        for (int i = 0; i < 10; i++)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stone.name = "Path Stone";
            stone.transform.SetParent(root.transform, false);
            stone.transform.position = new Vector3(Random.Range(-0.7f, 0.7f), 0.025f, -7f + i * 2.6f);
            stone.transform.rotation = Quaternion.Euler(0f, Random.Range(-8f, 8f), 0f);
            stone.transform.localScale = new Vector3(Random.Range(1.3f, 2.1f), 0.08f, Random.Range(1.3f, 2.2f));
            stone.GetComponent<Renderer>().material = pathMaterial;
        }

        // Warm lamps mark the road after dusk and make the hub read as a safe place.
        Material lampMaterial = CreateMaterial(new Color(0.32f, 0.22f, 0.12f));
        for (int i = 0; i < 5; i++)
        {
            float z = -4f + i * 5f;
            CreateLantern(root.transform, new Vector3(-3.5f, 0f, z), lampMaterial);
            CreateLantern(root.transform, new Vector3(3.5f, 0f, z), lampMaterial);
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.17f, 0.2f, 0.23f);
        RenderSettings.fogStartDistance = 34f;
        RenderSettings.fogEndDistance = 95f;
    }

    static void CreateLantern(Transform parent, Vector3 position, Material material)
    {
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.name = "Wayfinder Lantern Post";
        post.transform.SetParent(parent, false);
        post.transform.position = position + Vector3.up * 0.8f;
        post.transform.localScale = new Vector3(0.18f, 0.8f, 0.18f);
        Object.Destroy(post.GetComponent<Collider>());
        post.GetComponent<Renderer>().material = material;

        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flame.name = "Lantern Glow";
        flame.transform.SetParent(parent, false);
        flame.transform.position = position + Vector3.up * 1.75f;
        flame.transform.localScale = Vector3.one * 0.28f;
        Object.Destroy(flame.GetComponent<Collider>());
        SetMaterial(flame.GetComponent<Renderer>(), new Color(1f, 0.58f, 0.2f), true);
        Light light = flame.AddComponent<Light>();
        light.color = new Color(1f, 0.52f, 0.2f);
        light.intensity = 1.15f;
        light.range = 7f;
    }

    static void CreateSanctuaryChronicle()
    {
        GameObject plinth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plinth.name = "Sanctuary Chronicle";
        plinth.transform.position = new Vector3(-9f, 0.8f, 4f);
        plinth.transform.localScale = new Vector3(3f, 1.6f, 0.5f);
        SetMaterial(plinth.GetComponent<Renderer>(), new Color(0.17f, 0.16f, 0.2f));

        GameObject textObject = new GameObject("Sanctuary Record");
        textObject.transform.position = new Vector3(-9f, 1.3f, 3.72f);
        textObject.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        TextMesh text = textObject.AddComponent<TextMesh>();
        SavedGameProgress progress = GameProgression.Load();
        string[] classes = GameProgression.GetUnlockedClassNames();
        text.text = "SANCTUARY RECORD\nExpeditions: " + progress.completedRuns +
            "\nBest level: " + progress.bestRunLevel +
            "\nMemory shards: " + progress.masteryResourceBalance +
            "\nClasses: " + string.Join(", ", classes);
        text.fontSize = 28;
        text.characterSize = 0.055f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = new Color(0.9f, 0.86f, 0.72f);
    }

    static void CreateResident(string residentName, string dialogue, Vector3 position, Color color)
    {
        GameObject resident = new GameObject(residentName);
        resident.transform.position = position;

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(resident.transform, false);
        body.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        body.transform.localScale = new Vector3(0.75f, 0.95f, 0.75f);
        Object.Destroy(body.GetComponent<Collider>());
        SetMaterial(body.GetComponent<Renderer>(), color);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(resident.transform, false);
        head.transform.localPosition = new Vector3(0f, 2.05f, 0f);
        head.transform.localScale = Vector3.one * 0.55f;
        Object.Destroy(head.GetComponent<Collider>());
        SetMaterial(head.GetComponent<Renderer>(), new Color(0.72f, 0.62f, 0.52f));

        GameObject nameTag = new GameObject("Name Tag");
        nameTag.transform.SetParent(resident.transform, false);
        nameTag.transform.localPosition = new Vector3(0f, 2.65f, 0f);
        TextMesh text = nameTag.AddComponent<TextMesh>();
        text.text = residentName;
        text.fontSize = 40;
        text.characterSize = 0.07f;
        text.anchor = TextAnchor.LowerCenter;
        text.alignment = TextAlignment.Center;
        text.color = Color.white;

        HubResident interaction = resident.AddComponent<HubResident>();
        interaction.Initialize(residentName, dialogue);
    }

    static void SetMaterial(Renderer renderer, Color color, bool emissive = false)
    {
        Material material = CreateMaterial(color);
        material.color = color;
        if (emissive && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 2f);
        }
        renderer.material = material;
    }

    static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
