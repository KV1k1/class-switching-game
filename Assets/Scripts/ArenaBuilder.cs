using UnityEngine;

public class ArenaBuilder : MonoBehaviour
{
    public GameObject[] treePrefabs;
    public int treeCount = 120;
    public float halfSize = 35f;               // invisible walls stand this far from the center
    public float forestDepth = 12f;            // how far the trees extend beyond the walls
    public Vector2 scaleRange = new Vector2(0.8f, 1.4f);

    void Start()
    {
        BuildWalls();
        ScatterTrees();
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

    void ScatterTrees()
    {
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        float outer = halfSize + forestDepth;
        int placed = 0, tries = 0;

        while (placed < treeCount && tries < treeCount * 20)
        {
            tries++;
            Vector3 p = new Vector3(Random.Range(-outer, outer), 0f, Random.Range(-outer, outer));

            // keep the playable square clear, only plant in the ring outside it
            if (Mathf.Abs(p.x) < halfSize + 1f && Mathf.Abs(p.z) < halfSize + 1f) continue;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
            GameObject tree = Instantiate(prefab, transform.position + p,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
            tree.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);
            placed++;
        }
    }
}