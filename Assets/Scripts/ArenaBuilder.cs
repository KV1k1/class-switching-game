using UnityEngine;
using UnityEngine.SceneManagement;

public class ArenaBuilder : MonoBehaviour
{
    public GameObject[] treePrefabs;
    [Header("Forest village environment")]
    public GameObject[] forestTreePrefabs;
    public GameObject[] forestDetailPrefabs;
    public GameObject[] villageBuildings;
    public GameObject[] villageProps;
    public GameObject[] villageWalkwayPrefabs;
    public int forestTreeCount = 105;
    public int forestDetailCount = 55;
    public float forestInnerRadius = 38f;
    public float forestOuterRadius = 65f;
    public GameObject[] sanctuaryTreePrefabs;
    public GameObject[] sanctuaryDetailPrefabs;
    [Header("Archivist NPC")]
    public GameObject archivistPrefab;
    public AnimationClip archivistIdleClip;
    public AnimationClip archivistWalkingClip;
    public AnimationClip archivistGreetClip;
    public int treeCount = 120;
    public float halfSize = 35f;               // invisible walls stand this far from the center
    public float forestDepth = 12f;            // how far the trees extend beyond the walls
    public Vector2 scaleRange = new Vector2(0.8f, 1.4f);

    void Start()
    {
        // HubScene has its own woodland layout built by HubWorldController.
        // Keep this asset-backed tree catalog available there without copying
        // the expedition arena walls and forest ring into the sanctuary.
        if (SceneManager.GetActiveScene().name == "HubScene") return;

        BuildWalls();
        BuildForestVillage();
    }

    void BuildWalls()
    {
        float t = 2f;      // wall thickness
        float h = 10f;     // wall height
        float len = halfSize * 2f + t * 2f;

        MakeWall(new Vector3(0, h / 2, halfSize + t / 2), new Vector3(len, h, t));
        MakeWall(new Vector3(0, h / 2, -halfSize - t / 2), new Vector3(len, h, t));
        MakeWall(new Vector3(halfSize + t / 2, h / 2, 0), new Vector3(t, h, len));
        MakeWall(new Vector3(-halfSize - t / 2, h / 2, 0), new Vector3(t, h, len));
    }

    void MakeWall(Vector3 pos, Vector3 size)
    {
        GameObject w = new GameObject("Arena Wall");
        w.transform.SetParent(transform, false);
        w.transform.localPosition = pos;
        w.AddComponent<BoxCollider>().size = size;
    }

    void BuildForestVillage()
    {
        GameObject rootObject = new GameObject("First Expedition - Pinehollow Village");
        Transform root = rootObject.transform;
        Material trimMaterial = CreateMaterial(new Color(0.2f, 0.16f, 0.12f));
        AdjustGroundTexture();

        // Use the plank mesh as the actual walkway and leave the square open and grassy.
        // This avoids the raised cube tiles that obscured the village center.
        BuildRoad(root, new Vector3(-36f, 0.04f, -42f), new Vector3(-23f, 0.04f, -38f), false);
        BuildRoad(root, new Vector3(-23f, 0.04f, -38f), new Vector3(0f, 0.04f, -34f), true);
        BuildRoad(root, new Vector3(0f, 0.04f, -34f), new Vector3(0f, 0.04f, 39f), true);
        BuildRoad(root, new Vector3(-24f, 0.04f, -14f), new Vector3(24f, 0.04f, -14f), false);
        BuildRoad(root, new Vector3(-24f, 0.04f, 15f), new Vector3(24f, 0.04f, 15f), false);

        // Four cottages frame the square while leaving a large, uninterrupted route north.
        PlaceVillagePrefab(0, new Vector3(-27f, 0f, -19f), 45f, 1.15f, root, "Village Cottage - West Gate");
        PlaceVillagePrefab(1, new Vector3(27f, 0f, -19f), 315f, 1.15f, root, "Village Cottage - East Gate");
        PlaceVillagePrefab(2, new Vector3(-27f, 0f, 20f), 135f, 1.1f, root, "Village Cottage - North West");
        PlaceVillagePrefab(3, new Vector3(27f, 0f, 20f), 225f, 1.1f, root, "Village Cottage - North East");

        // Familiar village props give the plaza lived-in detail without blocking the combat lane.
        PlaceVillageProp(0, new Vector3(-11f, 0f, 9f), 0f, 1f, root, "Village Well");
        PlaceVillageProp(1, new Vector3(17f, 0f, -8f), 160f, 1f, root, "Village Supply Wagon");
        PlaceVillageProp(2, new Vector3(-14f, 0f, -8f), 35f, 1f, root, "Village Bench West");
        PlaceVillageProp(2, new Vector3(14f, 0f, 9f), 215f, 1f, root, "Village Bench East");
        PlaceVillageProp(3, new Vector3(-23f, 0f, -10f), 18f, 1f, root, "Village Supplies West");
        PlaceVillageProp(3, new Vector3(23f, 0f, 10f), 202f, 1f, root, "Village Supplies East");
        PlaceVillageProp(4, new Vector3(20f, 0f, -12f), 0f, 1f, root, "Village Barrel Stack");

        BuildVillageLanterns(root, trimMaterial);
        ScatterForestTrees(root);
        ScatterForestDetails(root);

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.28f, 0.34f, 0.31f);
        RenderSettings.fogStartDistance = 58f;
        RenderSettings.fogEndDistance = 145f;
    }

    void PlaceVillagePrefab(int index, Vector3 position, float yaw, float scale, Transform parent, string objectName)
    {
        if (villageBuildings == null || index >= villageBuildings.Length || villageBuildings[index] == null) return;
        GameObject building = Instantiate(villageBuildings[index], position,
            Quaternion.Euler(0f, yaw, 0f), parent);
        building.name = objectName;
        building.transform.localScale *= scale;
        AddBuildingColliders(building);
    }

    static void AddBuildingColliders(GameObject building)
    {
        foreach (MeshFilter meshFilter in building.GetComponentsInChildren<MeshFilter>())
        {
            if (meshFilter.sharedMesh == null || meshFilter.GetComponent<Collider>() != null) continue;
            BoxCollider blocker = meshFilter.gameObject.AddComponent<BoxCollider>();
            blocker.center = meshFilter.sharedMesh.bounds.center;
            blocker.size = meshFilter.sharedMesh.bounds.size;
        }
    }

    void PlaceVillageProp(int index, Vector3 position, float yaw, float scale, Transform parent, string objectName)
    {
        if (villageProps == null || index >= villageProps.Length || villageProps[index] == null) return;
        GameObject prop = Instantiate(villageProps[index], position,
            Quaternion.Euler(0f, yaw, 0f), parent);
        prop.name = objectName;
        prop.transform.localScale *= scale;
    }

    void BuildRoad(Transform parent, Vector3 start, Vector3 end, bool curve)
    {
        if (villageWalkwayPrefabs == null || villageWalkwayPrefabs.Length == 0) return;

        Vector3 delta = end - start;
        float length = delta.magnitude;
        // Each mesh is a short crosswise plank section; keep sections close so
        // the boards read as a continuous path instead of isolated pads.
        const float tileSpacing = 1.3f;
        int segments = Mathf.CeilToInt(length / tileSpacing);
        for (int i = 0; i < segments; i++)
        {
            float t = (i + 0.5f) / segments;
            Vector3 position = Vector3.Lerp(start, end, t);
            if (curve) position.x += Mathf.Sin(t * Mathf.PI * 1.4f) * 1.8f;

            float beforeT = Mathf.Max(0f, t - 0.01f);
            float afterT = Mathf.Min(1f, t + 0.01f);
            Vector3 before = Vector3.Lerp(start, end, beforeT);
            Vector3 after = Vector3.Lerp(start, end, afterT);
            if (curve)
            {
                before.x += Mathf.Sin(beforeT * Mathf.PI * 1.4f) * 1.8f;
                after.x += Mathf.Sin(afterT * Mathf.PI * 1.4f) * 1.8f;
            }
            Vector3 forward = (after - before).normalized;
            GameObject prefab = villageWalkwayPrefabs[i % villageWalkwayPrefabs.Length];
            if (prefab == null) continue;
            GameObject planks = Instantiate(prefab, position, Quaternion.LookRotation(forward), parent);
            planks.name = "Pinehollow Plank Walkway";
            planks.transform.localScale *= 0.72f;
        }
    }

    static void AdjustGroundTexture()
    {
        GameObject ground = GameObject.Find("ground");
        Renderer groundRenderer = ground != null ? ground.GetComponent<Renderer>() : null;
        if (groundRenderer == null) return;

        Material groundMaterial = groundRenderer.material;
        groundMaterial.mainTextureScale = new Vector2(40f, 40f);
        groundMaterial.color = new Color(0.9f, 0.97f, 0.84f, 1f);
    }

    void BuildVillageLanterns(Transform parent, Material postMaterial)
    {
        for (int i = 0; i < 4; i++)
        {
            float z = -21f + i * 13f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 position = new Vector3(side * 5.5f, 0f, z);
                GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                post.name = "Village Waylight Post";
                post.transform.SetParent(parent, false);
                post.transform.position = position + Vector3.up * 1.1f;
                post.transform.localScale = new Vector3(0.16f, 1.1f, 0.16f);
                Destroy(post.GetComponent<Collider>());
                post.GetComponent<Renderer>().sharedMaterial = postMaterial;

                GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lantern.name = "Village Waylight";
                lantern.transform.SetParent(parent, false);
                lantern.transform.position = position + Vector3.up * 2.35f;
                lantern.transform.localScale = Vector3.one * 0.28f;
                Destroy(lantern.GetComponent<Collider>());
                Renderer renderer = lantern.GetComponent<Renderer>();
                Material glow = CreateMaterial(new Color(1f, 0.61f, 0.27f));
                glow.EnableKeyword("_EMISSION");
                glow.SetColor("_EmissionColor", new Color(1f, 0.48f, 0.16f) * 1.8f);
                renderer.sharedMaterial = glow;

                Light light = lantern.AddComponent<Light>();
                light.color = new Color(1f, 0.58f, 0.3f);
                light.intensity = 1.15f;
                light.range = 8f;
            }
        }
    }

    void ScatterForestTrees(Transform parent)
    {
        GameObject[] trees = forestTreePrefabs != null && forestTreePrefabs.Length > 0
            ? forestTreePrefabs
            : treePrefabs;
        if (trees == null || trees.Length == 0) return;

        int placed = 0;
        int attempts = 0;
        while (placed < forestTreeCount && attempts < forestTreeCount * 18)
        {
            attempts++;
            Vector2 direction = Random.insideUnitCircle.normalized;
            float radius = Random.Range(forestInnerRadius, Mathf.Min(forestOuterRadius, halfSize - 3f));
            Vector3 position = new Vector3(direction.x * radius, 0f, direction.y * radius);

            // Leave a wide northbound route and an open southern entrance.
            if ((position.z > 29f && Mathf.Abs(position.x) < 13f) ||
                (position.z < -31f && Mathf.Abs(position.x) < 12f) ||
                IsNearForestTrail(position, 5.5f)) continue;

            GameObject prefab = trees[Random.Range(0, trees.Length)];
            if (prefab == null) continue;
            GameObject tree = Instantiate(prefab, position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
            tree.name = "Pinehollow Forest Tree";
            tree.transform.localScale *= Random.Range(0.9f, 1.25f);
            placed++;
        }
    }

    void ScatterForestDetails(Transform parent)
    {
        if (forestDetailPrefabs == null || forestDetailPrefabs.Length == 0) return;

        int placed = 0;
        int attempts = 0;
        while (placed < forestDetailCount && attempts < forestDetailCount * 12)
        {
            attempts++;
            Vector2 direction = Random.insideUnitCircle.normalized;
            float radius = Random.Range(29f, Mathf.Min(forestOuterRadius - 3f, halfSize - 5f));
            Vector3 position = new Vector3(direction.x * radius, 0f, direction.y * radius);
            if ((position.z > 26f && Mathf.Abs(position.x) < 15f) || IsNearForestTrail(position, 3f)) continue;

            GameObject prefab = forestDetailPrefabs[Random.Range(0, forestDetailPrefabs.Length)];
            if (prefab == null) continue;
            GameObject detail = Instantiate(prefab, position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), parent);
            detail.name = "Pinehollow Ferns and Stones";
            detail.transform.localScale *= Random.Range(0.8f, 1.2f);
            placed++;
        }
    }

    static bool IsNearForestTrail(Vector3 position, float clearance)
    {
        Vector2 point = new Vector2(position.x, position.z);
        return DistanceToSegment(point, new Vector2(-36f, -42f), new Vector2(-23f, -38f)) < clearance ||
            DistanceToSegment(point, new Vector2(-23f, -38f), new Vector2(0f, -34f)) < clearance;
    }

    static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= 0.0001f) return Vector2.Distance(point, start);
        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * t);
    }

    static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("Could not find a supported shader for the forest village materials.");
            return null;
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
